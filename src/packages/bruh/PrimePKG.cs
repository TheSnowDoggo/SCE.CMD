using QPrime;
using System.Text;

namespace CMD
{
    internal class PrimePKG : Package
    {
        private readonly PrimeFinder primeFinder;

        public PrimePKG()
        {
            primeFinder = new();

            Name = "Prime";
            Commands = new()
            {
                { "isprime", new(IsPrimeCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Determines whether the given list of numbers are prime." } },
                { "primecache", new(PrecacheCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Precaches all numbers up to the given number."} },
            };
        }

        private void PrecacheCMD(string[] args, Cmd.Callback cb)
        {
            if (!int.TryParse(args[0], out var n))
                throw new CmdException("Prime", $"Cannot convert \'{args[0]}\' to int.");
            if (!primeFinder.Precache(n))
                throw new CmdException("Prime", $"Already precached up to {primeFinder.CacheMax}.");
            cb.Launcher.FeedbackLine($"Sucessfully precached up to {n}.");
        }

        private void IsPrimeCMD(string[] args)
        {
            StringBuilder sb = new();
            foreach (var part in args[0].Split(','))
            {
                var range = part.Split('-');
                if (range.Length == 1)
                {
                    if (!double.TryParse(part, out double n))
                        throw new CmdException("Prime", $"Unable to convert \'{part}\' to int.");
                    bool prime = n % 1 == 0 && primeFinder.IsPrime((int)n);
                    sb.AppendLine($"{n} is {(prime ? "prime" : "not prime")}");
                }
                else
                {
                    if (range.Length == 2 && int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
                    {
                        sb.Append($"Primes[{start}-{end}]: ");

                        bool first = true;
                        int primes = 0;
                        for (int n = start; n <= end; ++n)
                        {
                            if (primeFinder.IsPrime(n))
                            {
                                if (!first)
                                    sb.Append(',');
                                sb.Append(n);
                                ++primes;
                                first = false;
                            }
                        }
                        sb.AppendLine($"\nThere are {primes} prime numbers between {start}-{end}.");
                    }
                    else
                    {
                        throw new CmdException("Prime", $"Range invalid \'{part}\'.");
                    }
                }
            }
            Console.Write(sb.ToString());
        }
    }
}
