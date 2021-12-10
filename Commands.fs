module VDCPSpoof.Commands
open Types
open System
open System.IO


let convertToMap (cmds:Command list)=
    cmds|>List.map(fun x->(x.commandType,x.commandCode),x)
    |>Map.ofList

let play msg=
    printfn "playing"
    [|0x04uy|]



let commands=
    [Command "play" 0x1uy 0x01uy play ]|>convertToMap




let handleMessage (port:Stream)  (msg:VDCPMessage)=
    async{
        match commands.TryFind(msg.cmdType,msg.commandCode) with
        |Some(cmd) ->
            let response=cmd.action msg
            do! port.AsyncWrite(response)
            ()
        |None->
            ()
    }