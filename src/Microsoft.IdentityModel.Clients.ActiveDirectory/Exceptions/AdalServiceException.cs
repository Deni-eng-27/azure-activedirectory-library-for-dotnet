﻿//----------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System.Globalization;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Http;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Http;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{

    /// <summary>
    /// The exception type thrown when the server returns an error. It's required to look at the internal details of the exception for a more information.
    /// </summary>
    public class AdalServiceException : AdalException
    {
        /// <summary>
        ///  Initializes a new instance of the exception class with a specified
        ///  error code and error message.
        /// </summary>
        /// <param name="errorCode">The protocol error code returned by the service or generated by client. This is the code you can rely on for exception handling.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public AdalServiceException(string errorCode, string message)
            : base(errorCode, message)
        {
        }

        /// <summary>
        ///  Initializes a new instance of the exception class with a specified
        ///  error code and a reference to the inner exception that is the cause of
        ///  this exception.
        /// </summary>
        /// <param name="errorCode">The protocol error code returned by the service or generated by client. This is the code you can rely on for exception handling.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified. It may especially contain the actual error message returned by the service.</param>
        internal AdalServiceException(string errorCode, Exception innerException)
            : this(errorCode, GetErrorMessage(errorCode), null, innerException)
        {
        }

        /// <summary>
        ///  Initializes a new instance of the exception class with a specified
        ///  error code, error message and a reference to the inner exception that is the cause of
        ///  this exception.
        /// </summary>
        /// <param name="errorCode">The protocol error code returned by the service or generated by client. This is the code you can rely on for exception handling.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="serviceErrorCodes">The specific error codes that may be returned by the service.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified. It may especially contain the actual error message returned by the service.</param>
        internal AdalServiceException(string errorCode, string message, string[] serviceErrorCodes, Exception innerException)
            : base(errorCode, message, (innerException is HttpRequestWrapperException) ? innerException.InnerException : innerException)
        {
            if (innerException is HttpRequestWrapperException httpRequestWrapperException)
            {
                IHttpWebResponse response = httpRequestWrapperException.WebResponse;
                if (response != null)
                {
                    StatusCode = (int)response.StatusCode;
                    Headers = response.Headers;
                }
                else if (innerException.InnerException is TaskCanceledException taskCanceledException)
                {
                    if (!taskCanceledException.CancellationToken.IsCancellationRequested)
                    {
                        StatusCode = (int)HttpStatusCode.RequestTimeout;
                    }
                    else
                    {
                        // There is no HttpStatusCode for user cancelation
                        StatusCode = 0;
                    }
                }
                else
                {
                    StatusCode = 0;
                }
            }

            ServiceErrorCodes = serviceErrorCodes;
        }

        /// <summary>
        /// Gets the status code returned from http layer. This status code is either the HttpStatusCode in the inner HttpRequestException response or
        /// NavigateError Event Status Code in browser based flow (See http://msdn.microsoft.com/en-us/library/bb268233(v=vs.85).aspx).
        /// You can use this code for purposes such as implementing retry logic or error investigation.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets the specific error codes that may be returned by the service.
        /// </summary>
        public string[] ServiceErrorCodes { get; set; }

        /// <summary>
        /// Contains headers from the response that indicated an error
        /// </summary>
        public HttpResponseHeaders Headers { get; set; }

        /// <summary>
        /// Creates and returns a string representation of the current exception.
        /// </summary>
        /// <returns>A string representation of the current exception.</returns>
        public override string ToString()
        {
            return base.ToString() + string.Format(CultureInfo.CurrentCulture, "\n\tStatusCode: {0}", StatusCode);
        }

    }
}