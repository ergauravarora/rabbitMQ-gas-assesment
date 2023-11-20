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
        sensorValues.Add(value);
            Console.WriteLine("new data recived .........................");
            Console.WriteLine($"Recived: {value.Name} {value.Element} {value.Value}");

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
           
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri("rabbitmq://localhost"), h => { });

                cfg.ReceiveEndpoint("sensor_queue", e =>
                {
                    e.Consumer<SensorEventConsumer>(() => new SensorEventConsumer(repository));
                });
            });

            await busControl.StartAsync();

            Console.WriteLine("Consumer app started. ");
            await Task.Delay(1000);
            Console.WriteLine("Now it will listen to requests for other applications  ");
            Console.ReadLine();

            await busControl.StopAsync();
        }
    }


}
