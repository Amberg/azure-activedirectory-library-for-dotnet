﻿//------------------------------------------------------------------------------
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.ADAL.Common;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    public class AcquireTokenSilentTests
    {
        [TestMethod]
        [TestCategory("AcquireTokenSilentTests")]
        public async Task AcquireTokenSilentServiceErrorTestAsync()
        {
            Sts sts = new AadSts();
            TokenCache cache = new TokenCache();
            TokenCacheKey key = new TokenCacheKey(sts.Authority, sts.ValidResource, sts.ValidClientId, TokenSubjectType.User, "unique_id", "displayable@id.com");
            cache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                RefreshToken = "something-invalid",
                ResourceInResponse = sts.ValidResource,
                Result = new AuthenticationResult("Bearer", "some-access-token", DateTimeOffset.UtcNow)
            };

            AuthenticationContext context = new AuthenticationContext(sts.Authority, sts.ValidateAuthority, cache);

            try
            {
                await context.AcquireTokenSilentAsync(sts.ValidResource, sts.ValidClientId, new UserIdentifier("unique_id", UserIdentifierType.UniqueId));
                Verify.Fail("AdalSilentTokenAcquisitionException was expected");
            }
            catch (AdalSilentTokenAcquisitionException ex)
            {
                Verify.AreEqual(AdalError.FailedToAcquireTokenSilently, ex.ErrorCode);
                Verify.AreEqual(AdalErrorMessage.FailedToRefreshToken, ex.Message);
                Verify.IsNotNull(ex.InnerException);
                Verify.IsTrue(ex.InnerException is AdalException);
                Verify.AreEqual(((AdalException)ex.InnerException).ErrorCode, "invalid_grant");
            }
            catch
            {
                Verify.Fail("AdalSilentTokenAcquisitionException was expected");
            }

        }
    }
}
