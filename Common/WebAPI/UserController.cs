using Common.Core;
using Common.Model;
using Common.Model.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.WebAPI
{
    public class UserController : BaseController
    {
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            string token = JwtToken.CreateToken(request.userName,request.password);
            return new StringResultObject(Request,token);
        }
    }
}
