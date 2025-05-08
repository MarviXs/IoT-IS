using System;
using Fei.Is.Api.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Services.PLUCode
{
    public class PLUCodeService(AppDbContext context)
    {
        private readonly AppDbContext _context = context;
        private readonly Random _random = new Random();
        private const int MAX_RETRIES = 5;

        public string GetPLUCode()
        {
            string pluCode;
            int retryCount = 0;

            do
            {
                if (retryCount >= MAX_RETRIES)
                {
                    throw new InvalidOperationException("Unable to generate a unique PLU Code after several attempts.");
                }

                pluCode = _random.Next(0, 100000).ToString("D5");

                retryCount++;
            } while (_context.Products.Any(p => p.PLUCode == pluCode));

            return pluCode;
        }

        public async Task<string> GetPLUCodeAsync(CancellationToken cancellationToken)
        {
            string pluCode;
            int retryCount = 0;

            do
            {
                if (retryCount >= MAX_RETRIES)
                {
                    throw new InvalidOperationException("Unable to generate a unique PLU Code after several attempts.");
                }

                pluCode = _random.Next(0, 100000).ToString("D5");

                retryCount++;
            } while (await _context.Products.AnyAsync(p => p.PLUCode == pluCode, cancellationToken: cancellationToken));

            return pluCode;
        }
    }
}
