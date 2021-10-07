using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CocApi.Cache;
using CocApi.Client;
using Microsoft.Extensions.Logging;

namespace MinionBot.Streamers
{
    public class CustomTimeToLiveProvider : TimeToLiveProvider
    {
        public CustomTimeToLiveProvider(ILogger<CustomTimeToLiveProvider> logger) : base(logger)
        {

        }

        protected override ValueTask<TimeSpan> TimeToLiveAsync<T>(ApiResponse<T> apiResponse) => new(TimeSpan.MinValue);

        protected override ValueTask<TimeSpan> TimeToLiveAsync<T>(Exception exception) => new(TimeSpan.FromSeconds(15));
    }
}
