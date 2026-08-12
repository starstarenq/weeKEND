using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace D05_콜렉션_
{
    public class MonsterDataCollection
    {
        [XmlElement("monster")]
        public List<MonsterData> Monsters { get; set; } = new List<MonsterData>();
    }

    [XmlRoot("MonsterDataSet")]
    public class MonsterDataSet
    {
        [XmlElement("Pokemon")]
        public List<MonsterData> Pokemons
        { get; set; } = new();
    }
    public class MonsterData
    {
        [XmlAttribute("id")]
        public string Id { get; set; } = string.Empty;

        [XmlElement("name")]
        public string Name { get; set; } = string.Empty;

        [XmlElement("type")]
        public string Type { get; set; } = string.Empty; // 일반, 중간보스, 최종보스 등

        [XmlElement("chapter")]
        public string Chapter { get; set; } = string.Empty; // 유년기, 청소년기, 성인

        [XmlElement("description")]
        public string Description { get; set; } = string.Empty;

        [XmlElement("stats")]
        public MonsterStats Stats { get; set; } = new MonsterStats();

        [XmlElement("rewards")]
        public MonsterRewards Rewards { get; set; } = new MonsterRewards();
    }
    public class MonsterStats
    {
        [XmlElement("hp")]
        public int Hp { get; set; }

        [XmlElement("atk")]
        public int Atk { get; set; }

        [XmlElement("def")]
        public int Def { get; set; }

        [XmlElement("speed")]
        public float Speed { get; set; }
    }

    // 하위 <rewards> 노드와 매핑됩니다.
    public class MonsterRewards
    {
        [XmlElement("exp")]
        public int Exp { get; set; }

        [XmlElement("death_crystal")]
        public int DeathCrystal { get; set; } // 회고인생 기획서 전용 재화
    }
    public class MonsterDataLoader
    {
        public static MonsterDataCollection LoadMonsterData(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"XML 파일을 찾을 수 없습니다: {filePath}");
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(MonsterDataCollection));
                using (StreamReader reader = new StreamReader(filePath))
                {
                    MonsterDataCollection data = (MonsterDataCollection)serializer.Deserialize(reader);
                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"XML 로드 중 오류 발생: {ex.Message}");
                return null;
            }
        }
    }

}
