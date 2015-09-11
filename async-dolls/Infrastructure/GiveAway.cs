using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace AsyncDolls
{
    public class GiveAway
    {
        public async Task WorthThousandDollars()
        {
            await Process.Start(@"C:\Program Files\Microsoft Office\Office15\POWERPNT.exe", Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), "Giveaway.pptx"));
        }
    }
}