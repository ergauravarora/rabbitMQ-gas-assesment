using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System;
using System.Collections.Generic;
using MassTransit;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace ConsoleApp1
{
    

public class SensorValue
{
    public DateTimeOffset MeasurementTime { get; set; }
    public string Name { get; set; } = "";
    public string Element { get; set; } = "";
    public double Value { get; set; } = 0;
    public string Status { get; set; } = "";
    public string Unit { get; set; } = "";
    public double Minimum { get; set; } = 0;
    public double Maximum { get; set; } = 0;
    public string Fermenter { get; set; } = "";
    public bool Actor { get; set; }
    public bool License { get; set; }
    public string Serial { get; set; } = "";
}

public interface ISensor
{
    string Name { get; }
    string Fermenter { get; }
    SensorValue GetValue(SensorValue value);
    List<string> ListSensorElements();
}

public interface ISensorRepository
{
    bool TryGetSensor(string sensorName, out ISensor sensor);
    IEnumerable<ISensor> GetSensorList(string fermenter = "");
    bool TryGetValue(ISensor sensor, string element, out SensorValue value);
    void AddSensorValue(SensorValue value); // Add this method
    }

public class Sensor : ISensor
{
    public string Name { get; }
    public string Fermenter { get; }
    private List<string> Elements { get; }

    public Sensor(string name, string fermenter, List<string> elements)
    {
        Name = name;
        Fermenter = fermenter;
        Elements = elements;
    }

    public SensorValue GetValue(SensorValue value)
    {
        // Implement your logic here to handle the provided SensorValue and return a result.
        // For simplicity, just returning the input value.
        return value;
    }

    public List<string> ListSensorElements()
    {
        return Elements;
    }
}

public class SensorRepository : ISensorRepository
{
    private readonly List<ISensor> sensors;
    private readonly List<SensorValue> sensorValues;
        private readonly RabbitMQ_DBEntities dbContext;
        RabbitMQ_DBEntities db = new RabbitMQ_DBEntities();

        public SensorRepository()
        {
        sensors = new List<ISensor>();
        sensorValues = new List<SensorValue>();
        this.dbContext = new RabbitMQ_DBEntities();
        }


    public void AddSensor(ISensor sensor)
    {
        sensors.Add(sensor);
    }

    public void AddSensorValue(SensorValue value)
    {
         tblSensor model = new tblSensor();
            model.Actor = value.Actor;
            model.Element = value.Element;
            model.Fermenter = value.Fermenter;
            model.License = value.License;
            model.Maximum = (decimal)value.Maximum;
            model.Minimum = (decimal)value.Minimum;
            model.Name = value.Name;
            model.Serial = value.Serial;
            model.Status = value.Status;
            model.Unit = value.Unit;
            model.Value = (decimal)value.Value;
            model.Unit = value.Unit;
            model.Unit = value.Unit;
            model.DateTimeOffset = value.MeasurementTime;            
            dbContext.tblSensors.Add(model);
            dbContext.SaveChanges();
        //dbContext.SensorValues.Add(value);
        //dbContext.SaveChanges();
    }

    public bool TryGetSensor(string sensorName, out ISensor sensor)
    {
        sensor = sensors.Find(s => s.Name == sensorName);
        return sensor != null;
    }

    public IEnumerable<ISensor> GetSensorList(string fermenter = "")
    {
        return string.IsNullOrEmpty(fermenter)
            ? sensors
            : sensors.FindAll(s => s.Fermenter == fermenter);
    }

    public bool TryGetValue(ISensor sensor, string element, out SensorValue value)
    {
        value = sensorValues.FindLast(sv => sv.Name == sensor.Name && sv.Element == element);
        return value != null;
    }
  
}

public class SensorEvent
{
    public SensorValue SensorValue { get; set; }
}

public class SensorEventConsumer : IConsumer<SensorEvent>
{
    private readonly ISensorRepository sensorRepository;

    public SensorEventConsumer(ISensorRepository repository)
    {
        sensorRepository = repository;
    }

    public async Task Consume(ConsumeContext<SensorEvent> context)
    {
        var sensorValue = context.Message.SensorValue;
        sensorRepository.AddSensorValue(sensorValue);

        // Additional processing logic can be added here.
    }
}

    class Program
    {
        
        static async Task Main()
        {
            var repository = new SensorRepository();

            try
            {
                var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(new Uri("rabbitmq://localhost"), h => { });

                    cfg.ReceiveEndpoint("sensor_queue", e =>
                    {
                        e.Consumer<SensorEventConsumer>(() => new SensorEventConsumer(repository));
                    });
                });

                await busControl.StartAsync();

                Console.WriteLine("MassTransit/RabbitMQ bus started...");
                Console.ReadLine();
                RabbitMQ_DBEntities db = new RabbitMQ_DBEntities();

                int SensorID = 0;
                Console.WriteLine("Enter SensorID=" + SensorID);
                SensorID = Convert.ToInt32(Console.ReadLine());

                var item1 = db.tblSensors.FirstOrDefault(m => m.SensorID == SensorID);
                if (item1 != null)
                {
                    Console.WriteLine("Sensor Detials list= " + item1.Name + " ," + item1.Actor + " ," + item1.DateTimeOffset + " ," + item1.Element + " ," + item1.Fermenter + " ," + item1.License + " ," + item1.Maximum + " ," + item1.Minimum + " ," + item1.Unit + " ," + item1.Value);
                    Console.ReadLine();
                }


                var sensorList = db.tblSensors.ToList();
                foreach (var item in sensorList)
                {
                    Console.WriteLine("Sensor Fetching list= "+item.Name +" ,"+ item.Actor + " ," + item.DateTimeOffset + " ," + item.Element + " ," + item.Fermenter + " ," + item.License + " ," + item.Maximum + " ," + item.Minimum + " ," + item.Unit + " ," + item.Value);
                    Console.ReadLine();
                }               

                // Simulate sensors publishing data every ~5 seconds
            var simulatedSensors = new List<ISensor>
            {
                new Sensor("Sensor01", "unused", new List<string> { "Temperature", "Pressure", "Volume" }),
                new Sensor("Sensor02", "Fermenter01", new List<string> { "CO2" }),
                new Sensor("Sensor03", "Fermenter02", new List<string> { "CO2" })
            };

                var random = new Random();

                while (true)
                {
                    foreach (var sensor in simulatedSensors)
                    {
                        var sensorValue = new SensorValue
                        {
                            MeasurementTime = DateTimeOffset.Now,
                            Name = sensor.Name,
                            Element = sensor.ListSensorElements().FirstOrDefault(),
                            Value = random.NextDouble() * 100, // Simulating random values
                            Status = "OK",
                            Unit = "unit",
                            Minimum = 0,
                            Maximum = 100,
                            Fermenter = sensor.Fermenter,
                            Actor = false,
                            License = false,
                            Serial = Guid.NewGuid().ToString()
                        };

                        await busControl.Publish(new SensorEvent { SensorValue = sensorValue });

                        // Sleep for ~5 seconds
                        await Task.Delay(5000);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }


}
