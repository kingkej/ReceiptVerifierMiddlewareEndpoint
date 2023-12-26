using AppleReceiptVerifierNET.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptVerifierEndpoint.Tests;
internal record VerifyReceiptResponseStub : VerifyReceiptResponse
{
    public VerifyReceiptResponseStub() : base(null, null, null, null, null, null!, 0)
    {
    }
}
