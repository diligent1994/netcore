using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Core
{
    public class ConfigSetting
    {

        public DatabaseConnectionOption MongodbConfigruation { get; private set; }

        #region Constructor
        public ConfigSetting(IConfiguration configuration)
        {
            InitSettings(configuration);
        }

        private void InitSettings(IConfiguration configuration)
        {
            #region Database Connection info
            this.MongodbConfigruation = configuration.GetSection("DatabaseSet:Mongodb").Get<DatabaseConnectionOption>();
            #endregion
        }
        #endregion
    }
}
