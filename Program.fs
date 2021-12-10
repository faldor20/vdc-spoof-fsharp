
module VDCPSpoof.main
open System.IO
open System
open System.IO.Ports
open Types
// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
let OpenPort () =
        let port = new System.IO.Ports.SerialPort ("COM3", 4800, Ports.Parity.None, 8, Ports.StopBits.One)
        port.Open ()
        port.DtrEnable   <- true
        port.RtsEnable   <- true
        port.ReadTimeout <- 1000
        port 
let readStart (port:Stream)=
    async{
        let! byte=port.AsyncRead(1)|>Async.map (Array.head>>int);
        if byte=0x02 then
            return Ok()
        else 
            printfn "warn: =Got byte {%X} which was not start of message"byte
            return  Error()
    }
let readMessage (port:Stream)=
    async{
        let! len =  port.AsyncRead(1)|>Async.map (Array.head>>int);
        
        let buf:byte array=Array.zeroCreate len
        let readCount = port.Read(buf,0,len)
        if readCount= len then
            let header,rest=buf|>Array.splitAt 2 
            let message,checkSum=rest|>Array.splitAt (rest.Length-1)
            
            let command1= Nibbles header[0]
            let commandCode=header[1]
            let msg=
                {
                    byteCount= uint8 len
                    cmdType=command1.fistNib
                    unitAddress=command1.secondNib
                    commandCode=commandCode
                    data=message
                    checkSum=checkSum[0]
                }
            return Ok(msg)
        else
            printfn "ERROR: failed to read full message only got %d of %d"readCount len
            return Error()
    }

    

let main=
    async{
        let port=OpenPort()
        let serialStream=port.BaseStream
        //This is nasty and i hate it.
        let! message=async.Bind(readStart serialStream,(fun x->
            match x with
            |Ok()->readMessage serialStream
            |_->async{return Error()}
            ))
        match message with
        | Ok(msg)->
            let result= Commands.handleMessage serialStream msg
            ()
        |Error()->()
    }
    
