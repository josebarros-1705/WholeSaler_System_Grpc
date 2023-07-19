using System.Text;
using SDServerTP2.Models;
using System.Threading.Channels;
using RabbitMQ.Client;

namespace SDServerTP2.Broker
{
    public class EmitEVENTS
    {
        private RabbitMQ.Client.IModel _channel;

        public EmitEVENTS()
        {

            var factory = new ConnectionFactory() {  };
            factory.HostName = "25.57.204.241";
            factory.Port = AmqpTcpEndpoint.UseDefaultPort;
            factory.UserName = "barros";
            factory.Password = "barros123";
            factory.VirtualHost = "/";

    

            _channel = factory.CreateConnection().CreateModel();

            _channel.ExchangeDeclare(exchange: "EVENTS", type: ExchangeType.Direct);
        }


        public void SendEventos(string rkey, string evento)    //recebe a routing key e o evento em formato json
        {
            var body = Encoding.UTF8.GetBytes(evento);

            //Enviar a mensagem codificada em UTF8 para o exchange Alertas
            _channel.BasicPublish(exchange: "EVENTS",
                             routingKey: rkey,
                             basicProperties: null,
                             body: body);
            Console.WriteLine($" [x] Enviado '{rkey}':'{evento}'");

        }


    }
}
