using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOps.Build.Api.Shared.Mappers
{
    public interface IMapper<A,B>
    {
        Task<A> Map(B from);
        Task<B> Map(A from);
    }
}
