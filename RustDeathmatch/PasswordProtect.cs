using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using System.Data;
using UnityEngine;
using Oxide.Core;
using Rust;

namespace Oxide.Plugins
{
    [Info("Password Protect Server", "Lederp", "1.0.0")]
    class PasswordProtect : RustPlugin
    {
        object CanClientLogin(Network.Connection connection)
        {
            
            return "Testing.";
        }
    }
}
