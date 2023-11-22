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

public class SensorEvent
{
    public SensorValue SensorValue { get; set; }
}

    public interface GetSensorDataRequest
    {
        object SensorName { get; }
        object ElementName { get; }
    }

    public interface GetSensorDataResponse
    {
        SensorValue SensorData { get; }
    }


    //class Program
    //{
    //    static async Task Main()
    //    {
    //        var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
    //        {
    //            cfg.Host(new Uri("rabbitmq://localhost"), h => { });
    //        });

    //        await busControl.StartAsync();

    //        Console.WriteLine("Simulator app started...");

    //        while (true)
    //        {
    //            Console.WriteLine("Enter sensor name:");
    //            var name = Console.ReadLine();

    //            Console.WriteLine("Enter sensor element:");
    //            var element = Console.ReadLine();

    //            Console.WriteLine("Enter sensor value:");
    //            if (double.TryParse(Console.ReadLine(), out var value))
    //            {
    //                var sensorValue = new SensorValue
    //                {
    //                    MeasurementTime = DateTimeOffset.Now,
    //                    Name = name,
    //                    Element = element,
    //                    Value = value,
    //                    Status = "OK",
    //                    Unit = "unit",
    //                    Minimum = 0,
    //                    Maximum = 100,
    //                    Fermenter = "unused",
    //                    Actor = false,
    //                    License = false,
    //                    Serial = Guid.NewGuid().ToString()
    //                };

    //                await busControl.Publish(new SensorEvent { SensorValue = sensorValue });

    //                Console.WriteLine($"Published: {sensorValue.Name} {sensorValue.Element} {sensorValue.Value}");
    //            }
    //            else
    //            {
    //                Console.WriteLine("Invalid value. Please enter a valid numeric value.");
    //            }

    //            // Sleep for ~5 seconds
    //            await Task.Delay(5000);
    //        }
    //    }
    //}

    class Program
    {
        static async Task Main()
        {
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri("rabbitmq://localhost"), h => { });
            });

            var busHandle = await busControl.StartAsync();

            Console.WriteLine("Simulator app started...");

            while (true)
            {
                Console.WriteLine("Enter sensor name:");
                var sensorName = Console.ReadLine();

                Console.WriteLine("Enter sensor element:");
                var elementName = Console.ReadLine();

                // Send a request to get sensor data
                var requestClient = busControl.CreateRequestClient<GetSensorDataRequest>(new Uri("rabbitmq://localhost/sensor_queue"));

                var response = await busControl.Request<GetSensorDataRequest, GetSensorDataResponse>(new { SensorName = sensorName, ElementName = elementName });

                Console.WriteLine($"Received Sensor Data: {response.Message.SensorData}");

                // Sleep for ~5 seconds
                await Task.Delay(5000);
            }

            await busHandle.StopAsync();
        }
    }


}
