using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MapDriver
{
    [Serializable]
    public class Scenario
    {
        public List<Unit> StarterPack { get; set; } = null;
        public string Name { get; set; } = "";

        public void Save(string path, string source = null)
        {
            if (source == null)
                source = $"{Path.GetTempPath()}\\spage\\{Path.GetFileNameWithoutExtension(path)}";

            using (FileStream stream = new FileStream($"{source}\\info.dat", FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
                stream.Close();
            }

            if (File.Exists(path))
                File.Delete(path);
            ZipFile.CreateFromDirectory(source, path);
        }

        public static Scenario Load(string path, string source = null)
        {
            if (source == null)
                source = $"{Path.GetTempPath()}\\spage\\{Path.GetFileNameWithoutExtension(path)}";

            if (Directory.Exists(source))
                Directory.Delete(source, true);
            ZipFile.ExtractToDirectory(path, source);

            using (FileStream stream = new FileStream($"{source}\\info.dat", FileMode.Open, FileAccess.Read))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                Scenario scenario = (Scenario)formatter.Deserialize(stream);
                stream.Close();
                return scenario;
            }
        }
    }
}
