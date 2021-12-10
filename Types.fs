module VDCPSpoof.Types
open System
open System.IO
open System.IO.Ports
module Async =
    let map f computation =
        async.Bind(computation, f >> async.Return)
    
type Nibbles(thisByte:byte)=
    ///returns first half of the byte
    member this.fistNib with get()= (byte)(thisByte &&&  0x0Fuy);
    ///returns second half of the byte
    member this.secondNib with get()= (byte)(thisByte >>> 4);
type VDCPMessage=
    {
      byteCount:uint8
      cmdType:uint8
      unitAddress:uint8
      commandCode:uint8
      data:uint8 array
      checkSum:uint8  
    }
type VDCPResponse= uint8[]
type vdcpAction= VDCPMessage->VDCPResponse
type Command=
    {
    name:string
    commandType:uint8
    commandCode:uint8
    action:vdcpAction
    }
let Command name cmdType code action=
    {name=name;commandType=cmdType;commandCode=code;action=action}