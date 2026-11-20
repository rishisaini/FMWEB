using AssetCare.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCare.Repository.Interfaces
{
    public interface IDbContextFactory
    {
        AssetCareContext CurrentContext { get; set; }
    }
}
