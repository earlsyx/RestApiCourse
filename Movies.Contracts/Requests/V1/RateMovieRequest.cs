using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Contracts.Requests.V1;
public class RateMovieRequest
{
    public required int Rating { get; init; }
}
