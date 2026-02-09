using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Extensions.Email
{
    public static class EmailAssetRegistry
    {

         public static readonly Dictionary<string, string> AssetMap = new()
        {
            { "build_hedge_logo.png", "assets/build_hedge_logo.png" }
        };

    }
}
