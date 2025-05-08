namespace Fei.Is.Api.Services.EANCode
{
    public class EANCodeService
    {
        private const string PREFIX = "859"; // GS1 prefix pro ČR/SR
        public string FromPlu(string pluCode)
        {
            var paddedPlu = pluCode.PadLeft(9, '0');
            var baseCode = PREFIX + paddedPlu;

            int sum = 0;
            for (int i = 0; i < baseCode.Length; i++)
            {
                int digit = baseCode[i] - '0';
                sum += digit * ((i % 2 == 0) ? 1 : 3);
            }

            var mod = sum % 10;
            var checkDigit = mod == 0 ? 0 : 10 - mod;

            return baseCode + checkDigit;
        }
    }
}
