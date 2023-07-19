using System.Text;
using Firebase.Auth;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using SDServerTP2Client;
using Grpc.Core.Interceptors;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using System.Data.SqlTypes;
using RabbitMQ.Client.Events;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using Google.Protobuf.WellKnownTypes;
using FirebaseAdmin.Auth;
using FirebaseAdmin;
using System.Runtime.Intrinsics.X86;

public class Program
{
    private const string API_KEY = "AIzaSyAp7aZOHEjDG_rQ5wrby4E-oUrrrCWxdxw";
    private static RabbitMQ.Client.IModel eventChannel;
    private static FirebaseAuthProvider firebaseAuthProvider;
    private static FirebaseAuthLink firebaseAuthLink;
    private static User loggedUser;

    public static async Task Main(string[] args)
    {
        //subscriber
        var factory = new ConnectionFactory() {};
        factory.HostName = "25.57.204.241";
        factory.Port = AmqpTcpEndpoint.UseDefaultPort;
        factory.UserName = "barros";
        factory.Password = "barros123";
        factory.VirtualHost = "/";


        eventChannel = factory.CreateConnection().CreateModel();

        eventChannel.ExchangeDeclare(exchange: "EVENTS", type: ExchangeType.Direct);

        var queueName = eventChannel.QueueDeclare().QueueName;

        //__________

        Console.OutputEncoding = Encoding.UTF8;

        firebaseAuthProvider = new FirebaseAuthProvider(new FirebaseConfig(API_KEY));

        Console.WriteLine("SISTEMA WHOLESALERS: LOGIN\n");

        Console.Write("Insira o username: ");
        string user = Console.ReadLine();

        Console.Write("Insira a password: ");
        string password = "";

        //password escondida com *
        do
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
            else
            {
                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine("");
                    break;

                }
            }
        } while (true);


        try
        {
            firebaseAuthLink = await firebaseAuthProvider.SignInWithEmailAndPasswordAsync(user, password);

            //Console.WriteLine(firebaseAuthLink.FirebaseToken);
            Console.WriteLine("\nAutenticado com Sucesso.\n\n");
            Thread.Sleep(1000);


        }
        catch (Exception ex)
        {
            Console.WriteLine("\nFalha na Autenticação!\n\n");
            Console.WriteLine(ex.Message);
            Console.ReadLine();
            return;
        }

        using var channel = GrpcChannel.ForAddress("http://25.57.204.241:5168");
        var client = new Pedidos.PedidosClient(channel);


        var headers = new Metadata();
        headers.Add("Authorization", $"Bearer {firebaseAuthLink.FirebaseToken}");

         loggedUser = await firebaseAuthProvider.GetUserAsync(firebaseAuthLink.FirebaseToken);
        MenuPrincipalAsync(client, headers, queueName);
    }

    public static async void MenuPrincipalAsync(Pedidos.PedidosClient client, Metadata headers, string queueName)
    {
        Console.Clear();
        Console.Write("Escolha o serviço a aceder:");
        if (loggedUser != null && loggedUser.DisplayName.Equals("Admin"))
        {
            Console.WriteLine("\n\t[0] - Zona Administrativa.");
        }
        Console.WriteLine(" \n\t[1] - Reservar Serviço.\n\t[2] - Ativar Serviço.\n\t[3] - Desativar Serviço.\n\t[4] - Terminar Serviço.\n\n\t[SAIR] para sair.\n\n");
        string op = Console.ReadLine();
        while (true)
        {
            switch (op)
            {
                case "0":
                    ZonaAdmin(client, headers, queueName);
                    Console.Clear();
                    break;
                case "1":
                    ReservarServico(client, headers);
                    Thread.Sleep(2000);
                    Console.WriteLine("\nPressione [enter] para sair.");
                    Console.ReadLine();
                    Console.Clear();
                    Console.Write("Escolha o serviço a aceder:");
                    if (loggedUser != null && loggedUser.DisplayName.Equals("Admin"))
                    {
                        Console.WriteLine("\n\t[0] - Zona Administrativa.");
                    }
                    Console.WriteLine(" \n\t[1] - Reservar Serviço.\n\t[2] - Ativar Serviço.\n\t[3] - Desativar Serviço.\n\t[4] - Terminar Serviço.\n\n\t[SAIR] para sair.\n\n");
                    break;
                case "2":
                    AtivarServico(client, headers, queueName);
                    Thread.Sleep(2000);
                    Console.WriteLine("\nPressione [enter] para sair.");
                    Console.ReadLine();
                    Console.Clear();
                    Console.Write("Escolha o serviço a aceder:");
                    if (loggedUser != null && loggedUser.DisplayName.Equals("Admin"))
                    {
                        Console.WriteLine("\n\t[0] - Zona Administrativa.");
                    }
                    Console.WriteLine(" \n\t[1] - Reservar Serviço.\n\t[2] - Ativar Serviço.\n\t[3] - Desativar Serviço.\n\t[4] - Terminar Serviço.\n\n\t[SAIR] para sair.\n\n");
                    break;
                case "3":
                    DesativarServico(client, headers, queueName);
                    Thread.Sleep(2000);
                    Console.WriteLine("\nPressione [enter] para sair.");
                    Console.ReadLine();
                    Console.Clear();
                    Console.Write("Escolha o serviço a aceder:");
                    if (loggedUser != null && loggedUser.DisplayName.Equals("Admin"))
                    {
                        Console.WriteLine("\n\t[0] - Zona Administrativa.");
                    }
                    Console.WriteLine(" \n\t[1] - Reservar Serviço.\n\t[2] - Ativar Serviço.\n\t[3] - Desativar Serviço.\n\t[4] - Terminar Serviço.\n\n\t[SAIR] para sair.\n\n");
                    break;
                case "4":
                    TerminarServico(client, headers, queueName);
                    Thread.Sleep(2000);
                    Console.WriteLine("\nPressione [enter] para sair.");
                    Console.ReadLine();
                    Console.Clear();
                    Console.Write("Escolha o serviço a aceder:");
                    if (loggedUser != null && loggedUser.DisplayName.Equals("Admin"))
                    {
                        Console.WriteLine("\n\t[0] - Zona Administrativa.");
                    }
                    Console.WriteLine(" \n\t[1] - Reservar Serviço.\n\t[2] - Ativar Serviço.\n\t[3] - Desativar Serviço.\n\t[4] - Terminar Serviço.\n\n\t[SAIR] para sair.\n\n");
                    break;

                case "SAIR":
                    System.Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("\nOpção Inválida.");
                    Thread.Sleep(1500);
                    Console.Clear();
                    Console.Write("Escolha o serviço a aceder:");
                    if (loggedUser != null && loggedUser.DisplayName.Equals("Admin"))
                    {
                        Console.WriteLine("\n\t[0] - Zona Administrativa.");
                    }
                    Console.WriteLine(" \n\t[1] - Reservar Serviço.\n\t[2] - Ativar Serviço.\n\t[3] - Desativar Serviço.\n\t[4] - Terminar Serviço.\n\n\t[SAIR] para sair.\n\n");
                    break;
            }

            op = Console.ReadLine();
        }
    }

    public static async void ReservarServico(Pedidos.PedidosClient client, Metadata headers)
    {
        Console.Clear();

        Console.WriteLine("\nIndique a modalidade a escolher:");
        string modalidade = Console.ReadLine();

        Console.WriteLine("\nIndique a rua do domicílio:");
        string rua = Console.ReadLine();

        Console.WriteLine("\nIndique o número de porta do domicílio:");
        string numeroporta = Console.ReadLine();
        try
        {
            var reply = await client.ReservasAsync(
                          new ModalidadeDomicilio { Mod = modalidade, Rua = rua, NPorta = numeroporta }, headers);

            if (reply != null && reply.CodAdm != null)
            {
                Console.WriteLine("Codigo Administrativo: " + reply.CodAdm + ".");
            }
            else
            {
                Console.WriteLine("Não foi possível ativar o serviço.");
            }

        }
        catch (RpcException ex)
        {
            Console.WriteLine(ex.StatusCode + ": Impossível RESERVAR");
        }
    }

    public static async void AtivarServico(Pedidos.PedidosClient client, Metadata headers, string queueName)
    {

        Console.Write("Insira o codigo administrativo: ");
        string codadm = Console.ReadLine();


        try
        {
            var reply = await client.AtivacaoAsync(
                          new CodigoAdministrativo { CodAdm = codadm }, headers);

            if (reply != null && reply.Conf == true)
            {
                Console.WriteLine("Confirmacao: " + reply.Conf + ". Tempo previsto: " + reply.Tempo + " segundos.");
                eventChannel.QueueBind(queue: queueName,
                             exchange: "EVENTS",
                             routingKey: "ACTIVE");

                Console.WriteLine(" [*] A receber mensagens...");

                var consumer = new EventingBasicConsumer(eventChannel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($" [x] Mensagem Recebida: {message}");
                };
                eventChannel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

               
            }
            else
            {
                Console.WriteLine("Não foi possível ativar o serviço.");
            }

        }
        catch (RpcException ex)
        {
            Console.WriteLine(ex.StatusCode + ": Impossível ATIVAR");
        }
    }

    public static async void DesativarServico(Pedidos.PedidosClient client, Metadata headers, string queueName)
    {

        Console.Write("Insira o codigo administrativo: ");
        string codadm = Console.ReadLine();

        try
        {
            var reply = await client.DesativacaoAsync(
                          new CodigoAdministrativo { CodAdm = codadm }, headers);

            if (reply != null && reply.Conf == true)
            {
                Console.WriteLine("Confirmacao: " + reply.Conf + ". Tempo previsto: " + reply.Tempo + " segundos.");
                eventChannel.QueueBind(queue: queueName,
                             exchange: "EVENTS",
                             routingKey: "DEACTIVATED");

                Console.WriteLine(" [*] A receber mensagens...");

                var consumer = new EventingBasicConsumer(eventChannel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($" [x] Mensagem Recebida: {message}");
                };
                eventChannel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

            }
            else
            {
                Console.WriteLine("Não foi possível desativar o serviço.");
            }

        }
        catch (RpcException ex)
        {
            Console.WriteLine(ex.StatusCode + ": Impossível DESATIVAR");
        }
    }

    public static async void TerminarServico(Pedidos.PedidosClient client, Metadata headers, string queueName)
    {

        Console.Write("Insira o codigo administrativo: ");
        string codadm = Console.ReadLine();

        try
        {
            var reply = await client.TerminacaoAsync(
                          new CodigoAdministrativo { CodAdm = codadm }, headers);

            if (reply != null && reply.Conf == true)
            {
                Console.WriteLine("Confirmacao: " + reply.Conf + ". Tempo previsto: " + reply.Tempo + " segundos.");
                eventChannel.QueueBind(queue: queueName,
                             exchange: "EVENTS",
                             routingKey: "TERMINATED");

                Console.WriteLine(" [*] A receber mensagens...");

                var consumer = new EventingBasicConsumer(eventChannel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($" [x] Mensagem Recebida: {message}");
                };
                eventChannel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine("\nPressione [enter] para sair.");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Não foi possível desativar o serviço.");
            }

        }
        catch (RpcException ex)
        {
            Console.WriteLine(ex.StatusCode + ": Impossível DESATIVAR");
        }
    }

    public static void ZonaAdmin(Pedidos.PedidosClient client, Metadata headers, string queueName)
    {
        Console.Clear();
        Console.Write("Escolha o serviço administrativo a aceder: \n\t[0] - Voltar.\n\t[1] - Gerir Operadores.\n\t[2] - Gerir Domicilios.\n\t[3] - Listar Serviços Ativos.\n\n\t[SAIR] para sair.\n\n");

        string op = Console.ReadLine();
        while (true)
        {
            switch (op)
            {
                case "0":
                    MenuPrincipalAsync(client, headers, queueName);
                    break;
                case "1":
                    GerirOperadores(client, headers, queueName);
                    break;
                case "2":
                    GerirDomicilios(client, headers, queueName);
                    break;
                case "3":
                    ListarServicos(client, headers);
                    Thread.Sleep(3000);
                    Console.WriteLine("\n[Enter] para voltar.");
                    Console.ReadLine();

                    Console.Clear();
                    Console.Write("Escolha o serviço administrativo a aceder: \n\t[0] - Voltar.\n\t[1] - Gerir Operadores.\n\t[2] - Gerir Domicilios.\n\t[3] - Listar Serviços Ativos.\n\n\t[SAIR] para sair.\n\n");

                    break;

                case "SAIR":
                    System.Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("\nOpção Inválida.");
                    Thread.Sleep(1500);
                    Console.Clear();
                    Console.Write("Escolha o serviço administrativo a aceder: \n\t[0] - Voltar.\n\t[1] - Gerir Operadores.\n\t[2] - Gerir Domicilios.\n\t[3] - Listar Serviços Ativos.\n\n\t[SAIR] para sair.\n\n");

                    break;
            }
            op = Console.ReadLine();
        }
    }

    public static void GerirDomicilios(Pedidos.PedidosClient client, Metadata headers, string queueName)
    {
        Console.Clear();
        Console.Write("Escolha o serviço administrativo a aceder: \n\t[0] - Voltar.\n\t[1] - Adicionar Domcilios.\n\t[2] - Adicionar Domicilios (CSV).\n\t[3] - Remover Domicilios.\n\t[4] - Ver Domicilios.\n\n\t[SAIR] para sair.\n\n");

        string op = Console.ReadLine();
        while (true)
        {
            switch (op)
            {
                case "0":
                    ZonaAdmin(client, headers, queueName);
                    Console.Clear();
                    break;
                case "1":
                    AdicionarDomicilios(client, headers);
                    Thread.Sleep(3000);
                    Console.WriteLine("\n[Enter] para voltar.");
                    Console.ReadLine();

                    Console.Clear();
                    Console.Write("Escolha o serviço administrativo a aceder: \n\t[0] - Voltar.\n\t[1] - Adicionar Domcilios.\n\t[2] - Adicionar Domicilios (CSV).\n\t[3] - Remover Domicilios.\n\t[4] - Ver Domicilios.\n\n\t[SAIR] para sair.\n\n");

                    break;
                case "2":
                    AdicionarDomiciliosCSV(client, headers);
                    Thread.Sleep(3000);
                    Console.WriteLine("\n[Enter] para voltar.");
                    Console.ReadLine();

                    Console.Clear();
                    Console.Write("Escolha o serviço administrativo a aceder: \n\t[0] - Voltar.\n\t[1] - Adicionar Domcilios.\n\t[2] - Adicionar Domicilios (CSV).\n\t[3] - Remover Domicilios.\n\t[4] - Ver Domicilios.\n\n\t[SAIR] para sair.\n\n");

                    break;
                case "3":
                    RemoverDomicilios(client, headers);
                    Thread.Sleep(3000);
                    Console.WriteLine("\n[Enter] para voltar.");
                    Console.ReadLine();

                    Console.Clear();
                    Console.Write("Escolha o serviço administrativo a aceder: \n\t[0] - Voltar.\n\t[1] - Adicionar Domcilios.\n\t[2] - Adicionar Domicilios (CSV).\n\t[3] - Remover Domicilios.\n\t[4] - Ver Domicilios.\n\n\t[SAIR] para sair.\n\n");

                    break;
                case "4":
                    ListarDomicilios(client, headers);
                    Thread.Sleep(3000);
                    Console.WriteLine("\n[Enter] para voltar.");
                    Console.ReadLine();

                    Console.Clear();
                    Console.Write("Escolha o serviço administrativo a aceder: \n\t[0] - Voltar.\n\t[1] - Adicionar Domcilios.\n\t[2] - Adicionar Domicilios (CSV).\n\t[3] - Remover Domicilios.\n\t[4] - Ver Domicilios.\n\n\t[SAIR] para sair.\n\n");

                    break;
                case "SAIR":
                    System.Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("\nOpção Inválida.");
                    Thread.Sleep(1500);
                    Console.Clear();
                    Console.Write("Escolha o serviço administrativo a aceder: \n\t[0] - Voltar.\n\t[1] - Adicionar Domcilios.\n\t[2] - Adicionar Domicilios (CSV).\n\t[3] - Remover Domicilios.\n\t[4] - Ver Domicilios.\n\n\t[SAIR] para sair.\n\n");

                    break;
            }
            op = Console.ReadLine();
        }
    }
 
    public static async void ListarServicos(Pedidos.PedidosClient client, Metadata headers)
    {
        Console.Clear();
        Console.WriteLine("\nServiços em Processo Ativo:\n");
        try
        {
            var reply = client.VerServicos(new AdminData { }, headers);
            await foreach (var servico in reply.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine($"{servico.Servs}");
            }

        }
        catch (RpcException ex)
        {
            Console.WriteLine(ex.StatusCode + ": Impossível Aceder ao Serviço.");
        }
    }

    public static async void AdicionarDomicilios(Pedidos.PedidosClient client, Metadata headers)
    {
        Console.Clear();
        Console.WriteLine("\nAdicionar Domicilios:\n");

        Console.WriteLine("Introduza o nome da rua que pretende adicionar: \n");
        var rua = Console.ReadLine();

        while (string.IsNullOrWhiteSpace(rua))
        {
            Console.WriteLine("\nNome de rua invalido, por favor introduza um novo nome: \n");
            rua = Console.ReadLine();
        }

        Console.WriteLine("\nIntroduza o numero da porta que pretende adicionar: \n");
        var porta = Console.ReadLine();

        while (string.IsNullOrWhiteSpace(porta))
        {
            Console.WriteLine("\nNumero de porta invalido, por favor introduza um novo: \n");
            rua = Console.ReadLine();
        }

        try
        {
            var reply = await client.AdicionarDomicilioAsync(
                          new DomicilioData { Rua = rua, NPorta = porta }, headers);

            if (reply != null && reply.Conf == true)
            {
                Console.WriteLine("Domicilio Adicionado.");
            }
            else
            {
                Console.WriteLine("Não foi possível adicionar o Domicilio.");
            }

        }
        catch (RpcException ex)
        {
            Console.WriteLine(ex.StatusCode + ": Impossível ADICIONAR");
        }
    }

    public static async void RemoverDomicilios(Pedidos.PedidosClient client, Metadata headers)
    {
        Console.Clear();
        Console.WriteLine("\nRemover Domicilios:\n");

        Console.WriteLine("Introduza o nome da rua que pretende remover: \n");
        var rua = Console.ReadLine();

        while (string.IsNullOrWhiteSpace(rua))
        {
            Console.WriteLine("\nNome de rua invalido, por favor introduza um novo nome: \n");
            rua = Console.ReadLine();
        }

        Console.WriteLine("\nIntroduza o numero da porta que pretende remover: \n");
        var porta = Console.ReadLine();

        while (string.IsNullOrWhiteSpace(porta))
        {
            Console.WriteLine("\nNumero de porta invalido, por favor introduza um novo: \n");
            rua = Console.ReadLine();
        }

        try
        {
            var reply = await client.RemoverDomiciliosAsync(
                          new DomicilioData { Rua = rua, NPorta = porta }, headers);

            if (reply != null && reply.Conf == true)
            {
                Console.WriteLine("Domicilio Removido.");
            }
            else
            {
                Console.WriteLine("Não foi possível remover o Domicilio.");
            }

        }
        catch (RpcException ex)
        {
            Console.WriteLine(ex.StatusCode + ": Impossível REMOVER");
        }
    }

    public static async void ListarDomicilios(Pedidos.PedidosClient client, Metadata headers)
    {
        Console.Clear();
        Console.WriteLine("\nDomicilios na Cobertura:\n");
        try
        {
            var reply = client.VerDomicilios(new AdminData { }, headers);
            await foreach (var domicilios in reply.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine($"{domicilios.Domicilios}");
            }

        }
        catch (RpcException ex)
        {
            Console.WriteLine(ex.StatusCode + ": Impossível Aceder ao Serviço.");
        }
    }

    public static void GerirOperadores(Pedidos.PedidosClient client, Metadata headers, string queueName)
    {
        Console.Clear();
        Console.Write("Escolha o serviço administrativo a aceder: \n\t[0] - Voltar.\n\t[1] - Adicionar Operadores.\n\t[2] - Remover Operdaores.\n\n\t[SAIR] para sair.\n\n");

        string op = Console.ReadLine();
        while (true)
        {
            switch (op)
            {
                case "0":
                    ZonaAdmin(client, headers, queueName);
                    Console.Clear();
                    break;
                case "1":
                    AdicionarOperador(headers);
                    Thread.Sleep(3000);
                    Console.WriteLine("\n[Enter] para voltar.");
                    Console.ReadLine();

                    Console.Clear();
                    Console.Write("Escolha o serviço administrativo a aceder: \n\t[0] - Voltar.\n\t[1] - Adicionar Operadores.\n\t[2] - Remover Operdaores.\n\n\t[SAIR] para sair.\n\n");

                    break;
                case "2":
                    RemoverOperador(headers);
                    Thread.Sleep(3000);
                    Console.WriteLine("\n[Enter] para voltar.");
                    Console.ReadLine();

                    Console.Clear();
                    Console.Write("Escolha o serviço administrativo a aceder: \n\t[0] - Voltar.\n\t[1] - Adicionar Operadores.\n\t[2] - Remover Operdaores.\n\n\t[SAIR] para sair.\n\n");

                    break;
               
                case "SAIR":
                    System.Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("\nOpção Inválida.");
                    Thread.Sleep(1500);
                    Console.Clear();
                    Console.Write("Escolha o serviço administrativo a aceder: \n\t[0] - Voltar.\n\t[1] - Adicionar Operadores.\n\t[2] - Remover Operdaores.\n\n\t[SAIR] para sair.\n\n");

                    break;
            }
            op = Console.ReadLine();
        }
    }


    public static async void AdicionarOperador(Metadata headers)
    {
        Console.Clear();
        Console.WriteLine("\nAdicionar Operador:\n");

        Console.WriteLine("Introduza o email do operador que pretende adicionar: \n");
        var user = Console.ReadLine();

        while (string.IsNullOrWhiteSpace(user))
        {
            Console.WriteLine("\nEmail invalido, por favor introduza um novo: \n");
            user = Console.ReadLine();
        }

        Console.WriteLine("\nIntroduza a password do operador que pretende adicionar: \n");
        var pass = "";

        //password escondida com *
        do
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                pass += key.KeyChar;
                Console.Write("*");
            }
            else
            {
                if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass = pass.Substring(0, pass.Length - 1);
                    Console.Write("\b \b");
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine("");
                    break;

                }
            }
        } while (true);

        while (string.IsNullOrWhiteSpace(pass))
        {
            Console.WriteLine("\nPassword invalida, por favor introduza uma nova: \n");
            //password escondida com *
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, pass.Length - 1);
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine("");
                        break;

                    }
                }
            } while (true);
        }

        try
        {
            OperacaoAutenticada(user, pass, "ADICIONAR");
        }
        catch (Exception ex)
        {
            Console.WriteLine("\nFalha na Criação!\n\n");
            Console.WriteLine(ex.Message);
            Console.ReadLine();
            return;
        }

    }

    public static void RemoverOperador(Metadata headers)
    {
        Console.Clear();
        Console.WriteLine("\nRemover Operador:\n");

        Console.WriteLine("Introduza o email do operador que pretende remover: \n");
        var user = Console.ReadLine();

        while (string.IsNullOrWhiteSpace(user))
        {
            Console.WriteLine("\nEmail invalido, por favor introduza um novo: \n");
            user = Console.ReadLine();
        }

        Console.WriteLine("\nIntroduza a password do operador que pretende remover: \n");
        var pass = "";

        //password escondida com *
        do
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                pass += key.KeyChar;
                Console.Write("*");
            }
            else
            {
                if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass = pass.Substring(0, pass.Length - 1);
                    Console.Write("\b \b");
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine("");
                    break;

                }
            }
        } while (true);

        while (string.IsNullOrWhiteSpace(pass))
        {
            Console.WriteLine("\nPassword invalida, por favor introduza novamente: \n");
            //password escondida com *
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, pass.Length - 1);
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine("");
                        break;

                    }
                }
            } while (true);
        }


        try
        {
            OperacaoAutenticada(user, pass, "REMOVER");
        }
        catch (Exception ex)
        {
            Console.WriteLine("\nFalha na Remoção!\n\n");
            Console.WriteLine(ex.Message);
            Console.ReadLine();
            return;
        }

    }

    public static async void OperacaoAutenticada(string user, string pass, string op)
    {
        if (loggedUser != null && loggedUser.DisplayName.Equals("Admin"))
        {
            switch (op)
            {
                case "REMOVER":
                    try
                    {
                        var opAuthLink = await firebaseAuthProvider.SignInWithEmailAndPasswordAsync(user, pass);
                        if (opAuthLink != null)
                        {
                            await firebaseAuthProvider.DeleteUserAsync(opAuthLink.FirebaseToken);

                            Console.WriteLine("\nRemovido com Sucesso.\n\n");
                            Thread.Sleep(1000);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return;
                    }
                    break;
                case "ADICIONAR":
                    try
                    {
                        var opAuthLink = await firebaseAuthProvider.CreateUserWithEmailAndPasswordAsync(user, pass, "Operador");
                        if (opAuthLink != null)
                        {
                            Console.WriteLine("\nCriado com Sucesso.\n\n");
                            Thread.Sleep(1000);
                            return;

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\nFalha na Criação!\n\n");
                        Console.WriteLine(ex.Message);
                        return;
                    }
                    break;
            }
        }
        else
        {
            Console.WriteLine("\nNão possui permissão para a operação!\n\n");
            Console.ReadLine();
            return;
        }

    }
    public static async void AdicionarDomiciliosCSV(Pedidos.PedidosClient client,Metadata headers)
    {
        
        Console.WriteLine("\nInsira o nome do ficheiro:\n");
        var filename = Console.ReadLine();
        while(string.IsNullOrWhiteSpace(filename))
        {
            filename = Console.ReadLine();
        }
        try
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    string linha = reader.ReadLine();
                    string[] campos = linha.Split(',');

                    var reply = new DomicilioData()
                    {
                        Rua = campos[0],
                        NPorta= campos[1]
                    };
                    await client.AdicionarDomicilioAsync(reply, headers);
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine("Erro! O ficheiro pode não existir");
            Console.WriteLine(ex.Message);
            Console.ReadLine();
            return;
        }

    }
}