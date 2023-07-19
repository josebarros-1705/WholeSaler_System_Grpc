using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using SDServerTP2;
using SDServerTP2.Broker;
using SDServerTP2.Data;
using SDServerTP2.Models;
using System;
using System.Data.Entity;
using System.Diagnostics.Metrics;

namespace SDServerTP2.Services
{
    public class PedidosService : Pedidos.PedidosBase
    {

        private readonly ILogger<PedidosService> _logger;
        private readonly SDServerTP2Context _context;
        private readonly EmitEVENTS _eventsbroker;
        public PedidosService(SDServerTP2Context context, EmitEVENTS eventsbroker, ILogger<PedidosService> logger)
        {
            _context = context;
            _eventsbroker = eventsbroker;
            _logger = logger;
        }

        public override Task<CodigoAdministrativo> Reservas(ModalidadeDomicilio modDom, ServerCallContext context)
        {
            var user = context.GetHttpContext().User;
            var email = user.Claims.SingleOrDefault(c => c.Type.Equals("email")).Value.ToString();


            if (user != null && user.Identity.IsAuthenticated)
            {
                if (_context.Domicilio == null && _context.Servico == null)
                {
                    return null;
                }

                string ruaNormalized = modDom.Rua.Replace(" ", "_");
                string codadmin = string.Join(".", modDom.Mod, ruaNormalized, modDom.NPorta);


               
                if (!_context.Domicilio.Any(d => d.Rua == modDom.Rua && d.NPorta == modDom.NPorta))
                {
                    throw new RpcException(new Status(StatusCode.NotFound, codadmin));
         
                }

               Domicilio dom =  _context.Domicilio.Single(d => d.Rua.Equals(modDom.Rua) && d.NPorta.Equals(modDom.NPorta));
              

                Servicos serv = new Servicos
                {
                    OperadorUsername = user.Claims.SingleOrDefault(c => c.Type.Equals("email")).Value.ToString(),
                    Domicilio = dom,
                    DomicilioId = dom.Id,
                    CodAdministrativo = codadmin,
                    Modalidade = modDom.Mod,
                    Estado = "RESERVED"
                };


                if (!_context.Servico.Any(s => s.CodAdministrativo.Equals(serv.CodAdministrativo) && s.Estado.Equals("RESERVED")))
                {
                    _context.Servico.Add(serv);
                    _context.SaveChangesAsync();

                    //guarda num ficheiro de logs
                    string logFile = "logs.csv";
                    string log = string.Join(";", email, codadmin, "RESERVED");

                    if (File.Exists(logFile))
                    {
                        using (StreamWriter sw = new StreamWriter(logFile, append: true))
                        {
                            sw.WriteLine(log);
                        }
                    }
                    else
                    {
                        using (FileStream fs = File.Create(logFile))
                        {
                            fs.Close();
                        }

                        using (StreamWriter sw = new StreamWriter(logFile, append: true))
                        {
                            sw.WriteLine(log);
                        }

                    }

                    return Task.FromResult(new CodigoAdministrativo
                    {
                        CodAdm = codadmin
                    });
                }


                throw new RpcException(new Status(StatusCode.AlreadyExists, codadmin));
            }

            return null;

        }

        public override Task<Confirmacao> Ativacao(CodigoAdministrativo cAdm, ServerCallContext context)
        {
            var user = context.GetHttpContext().User;
            var email = user.Claims.SingleOrDefault(c => c.Type.Equals("email")).Value.ToString();
            var service = _context.Servico.SingleOrDefault(s => s.CodAdministrativo == cAdm.CodAdm);

            if(service != null && (service.Estado == "DEACTIVATED" || service.Estado == "RESERVED") && email.Equals(service.OperadorUsername))
            {
                Random rnd = new Random();
                int delay = (rnd.Next(5, 21) * 1000); //alatorio entre 5 e 20, multiplicado por 1000 para milisegundos


            service.Estado = "ACTIVE";
                _context.Servico.Update(service);
                _context.SaveChangesAsync();

                //envia notificação numa thread nova
                new Thread(delegate () {
                    Thread.Sleep(delay);
                    _eventsbroker.SendEventos("ACTIVE", "Serviço ATIVO");
                }).Start();

                //guarda num ficheiro de logs
                string logFile = "logs.csv";
                string log = string.Join(";", email, cAdm.CodAdm, "ACTIVE");

                    if (File.Exists(logFile))
                    {
                        using (StreamWriter sw = new StreamWriter(logFile, append: true))
                        {
                            sw.WriteLine(log);
                        }
                    }
                    else
                    {
                        using (FileStream fs = File.Create(logFile))
                        {
                         fs.Close();
                        }

                        using (StreamWriter sw = new StreamWriter(logFile, append: true))
                        {
                            sw.WriteLine(log);
                        }

                }

                return Task.FromResult(new Confirmacao { Conf = true , Tempo = (delay/1000)});
            }
            else
            {
                return Task.FromResult(new Confirmacao { Conf = false, Tempo = 0 });
            }
        }

        public override Task<Confirmacao> Desativacao(CodigoAdministrativo cAdm, ServerCallContext context)
        {
            var user = context.GetHttpContext().User;
            var email = user.Claims.SingleOrDefault(c => c.Type.Equals("email")).Value.ToString();
            var service = _context.Servico.SingleOrDefault(s => s.CodAdministrativo == cAdm.CodAdm);

            if (service != null && (service.Estado == "ACTIVE") && email.Equals(service.OperadorUsername))
            {
                Random rnd = new Random();
                int delay = (rnd.Next(5, 21) * 1000); //alatorio entre 5 e 20, multiplicado por 1000 para milisegundos
                Thread.Sleep(delay);

                service.Estado = "DEACTIVATED";
                _context.Servico.Update(service);
                _context.SaveChangesAsync();
               
                //envia notificação numa thread nova
                new Thread(delegate () {
                    Thread.Sleep(delay);
                    _eventsbroker.SendEventos("DEACTIVATED", "Serviço DESATIVADO");
                }).Start();

                //guarda num ficheiro de logs
                string logFile = "logs.csv";
                string log = string.Join(";", email, cAdm.CodAdm, "DEACTIVATED");

                if (File.Exists(logFile))
                {
                    using (StreamWriter sw = new StreamWriter(logFile, append: true))
                    {
                        sw.WriteLine(log);
                    }
                }
                else
                {
                    using (FileStream fs = File.Create(logFile))
                    {
                        fs.Close();
                    }

                    using (StreamWriter sw = new StreamWriter(logFile, append: true))
                    {
                        sw.WriteLine(log);
                    }

                }

                return Task.FromResult(new Confirmacao { Conf = true , Tempo = (delay / 1000) });
            }
            else
            {
                return Task.FromResult(new Confirmacao { Conf = false, Tempo = 0 });
            }
        }


        public override Task<Confirmacao> Terminacao(CodigoAdministrativo cAdm, ServerCallContext context)
        {
            var user = context.GetHttpContext().User;
            var email = user.Claims.SingleOrDefault(c => c.Type.Equals("email")).Value.ToString();
            var service = _context.Servico.Where(s => s.CodAdministrativo == cAdm.CodAdm).SingleOrDefault();

           
            if (service != null && (service.Estado == "DEACTIVATED") && email.Equals(service.OperadorUsername))
            {
                Random rnd = new Random();
                int delay = (rnd.Next(5, 21) * 1000); //alatorio entre 5 e 20, multiplicado por 1000 para milisegundos

                _context.Remove(service);
                _context.SaveChangesAsync();
                
                //envia notificação numa thread nova
                new Thread(delegate () {
                    Thread.Sleep(delay);
                    _eventsbroker.SendEventos("TERMINATED", "Serviço TERMINADO");
                }).Start();

                //guarda num ficheiro de logs
                string logFile = "logs.csv";
                string log = string.Join(";", email, cAdm.CodAdm, "TERMINATED");

                if (File.Exists(logFile))
                {
                    using (StreamWriter sw = new StreamWriter(logFile, append: true))
                    {
                        sw.WriteLine(log);
                    }
                }
                else
                {
                    using (FileStream fs = File.Create(logFile))
                    {
                        fs.Close();
                    }

                    using (StreamWriter sw = new StreamWriter(logFile, append: true))
                    {
                        sw.WriteLine(log);
                    }

                }

                return Task.FromResult(new Confirmacao { Conf = true, Tempo = (delay / 1000) });
            }
            else
            {      
                return Task.FromResult(new Confirmacao { Conf = false, Tempo = 0 });
            }

        }

        public override async Task<ListaServicos> VerServicos(AdminData ad, IServerStreamWriter<ListaServicos> replyStream, ServerCallContext context)
        {
            var user = context.GetHttpContext().User;
            if(user.Claims.SingleOrDefault(c => c.Type.Equals("name")).Value.Equals("Admin"))
            {
                if (_context.Servico == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, ad.ToString()));
                }

                foreach (var s in _context.Servico.ToList())
                {
                    await replyStream.WriteAsync(new ListaServicos { Servs = string.Join(" - ",s.CodAdministrativo,s.Estado) });
                }
                return null;
            }else{
                throw new RpcException(new Status(StatusCode.PermissionDenied, ad.ToString()));
            }

        }

        public override Task<Confirmacao> AdicionarDomicilio(DomicilioData domData, ServerCallContext context)
        {
            var user = context.GetHttpContext().User;
            if (user.Claims.SingleOrDefault(c => c.Type.Equals("name")).Value.Equals("Admin"))
            {
                

                if (_context.Domicilio.Any(d => d.Rua.Equals(domData.Rua) && d.NPorta.Equals(domData.NPorta)))
                {
                    var domExistente = domData.Rua + "-" + domData.NPorta;
                    throw new RpcException(new Status(StatusCode.AlreadyExists, domExistente));
                }

                Domicilio domNovo = new Domicilio { Rua = domData.Rua, NPorta = domData.NPorta };

                if (domNovo != null)
                {
                    _context.Domicilio.Add(domNovo);
                    _context.SaveChangesAsync();
                    return Task.FromResult(new Confirmacao { Conf = true, Tempo = 0 });
                }

                return Task.FromResult(new Confirmacao { Conf = false, Tempo = 0 });
            }
            var userMail = user.Claims.SingleOrDefault(c => c.Type.Equals("email")).Value.ToString();
            throw new RpcException(new Status(StatusCode.PermissionDenied, userMail));
        }
        public override Task<Confirmacao> RemoverDomicilios(DomicilioData request, ServerCallContext context)
        {
            var user = context.GetHttpContext().User;
            if (user.Claims.SingleOrDefault(c => c.Type.Equals("name")).Value.Equals("Admin"))
            {

                if (!_context.Domicilio.Any(d => d.Rua.Equals(request.Rua) && d.NPorta.Equals(request.NPorta)))
                {
                    var dom = request.Rua + "-" + request.NPorta;
                    throw new RpcException(new Status(StatusCode.NotFound, dom));
                }
                var selected_dom = _context.Domicilio.SingleOrDefault(d => (d.Rua == request.Rua) && (d.NPorta == request.NPorta));
                if (selected_dom != null)
                {
                    //domicilio existe e é removido
                    _context.Remove(selected_dom);
                    _context.SaveChangesAsync();
                    return Task.FromResult(new Confirmacao { Conf = true, Tempo = 0 });
                }
                return Task.FromResult(new Confirmacao { Conf = false, Tempo = 0 });
            }
            else
            {
                //sem permissão
                return Task.FromResult(new Confirmacao { Conf = false, Tempo = 0 });
            }
        }
        public override async Task<ListaDomicilios> VerDomicilios(AdminData ad, IServerStreamWriter<ListaDomicilios> replyStream, ServerCallContext context)
        {
            var user = context.GetHttpContext().User;
            if (user.Claims.SingleOrDefault(c => c.Type.Equals("name")).Value.Equals("Admin"))
            {
                if(_context.Domicilio == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, ad.ToString()));
                }

                foreach (var d in _context.Domicilio.ToList())
                {
                    await replyStream.WriteAsync(new ListaDomicilios { Domicilios = string.Join(" - ", d.Rua, d.NPorta) });
                }
                return null;
            }
            else
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, ad.ToString()));
            }

           

        }
    }
}