using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Html5MobileDemos.Hubs
{
    public class MobileDemoHub : Hub
    {
        private static readonly ConnectionMapping<string> Servers = new ConnectionMapping<string>();

        public void CreateServer(string name)
        {
            Servers.Add(name, Context.ConnectionId);
        }

        public void SendRotationData(RotationData data, string serverName)
        {
            Clients.Client(GetConnectionIdFromServerName(serverName)).ReceiveRotationData(new
            {
                rotation = data,
                connectionId = Context.ConnectionId
            });
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Servers.Remove(Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        public List<string> GetServers()
        {
            return Servers.GetAllConnections();
        }


        public void Connect(string serverName)
        {
            Clients.Client(GetConnectionIdFromServerName(serverName)).ReceiveConnection(Context.ConnectionId);
        }

        public void SendAccelerationData(AccelerationData data, string serverName)
        {
            Clients.Client(GetConnectionIdFromServerName(serverName)).ReceiveAccelerationData(new
            {
                acceleration = data,
                connectionId = Context.ConnectionId
            });
        }

        private string GetConnectionIdFromServerName(string serverName)
        {
            return Servers.GetConnections(serverName).FirstOrDefault();
        }
    }

    public class MotionData
    {
        public AccelerationData Acceleration { get; set; }
        public RotationData Rotation { get; set; }
    }

    public class AccelerationData
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
    }

    public class RotationData
    {
        public double alpha { get; set; }
        public double beta { get; set; }
        public double gamma { get; set; }
    }

    public class ConnectionMapping<T>
    {
        private readonly Dictionary<T, HashSet<string>> _connections =
            new Dictionary<T, HashSet<string>>();

        public int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        public List<T> GetAllConnections()
        {
            return _connections.Keys.ToList();
        } 

        public void Add(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    connections = new HashSet<string>();
                    _connections.Add(key, connections);
                }

                lock (connections)
                {
                    connections.Add(connectionId);
                }
            }
        }

        public IEnumerable<string> GetConnections(T key)
        {
            HashSet<string> connections;
            if (_connections.TryGetValue(key, out connections))
            {
                return connections;
            }

            return Enumerable.Empty<string>();
        }

        public void Remove(string connectionId)
        {
            var keys = _connections.Where(c => c.Value.Contains(connectionId)).Select(c => c.Key).ToList();
            foreach (var server in keys)
            {
                Remove(server, connectionId);
            }
        }

        public void Remove(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    return;
                }

                lock (connections)
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                    {
                        _connections.Remove(key);
                    }
                }
            }
        }
    }
}