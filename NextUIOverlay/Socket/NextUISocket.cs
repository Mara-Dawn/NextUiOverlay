using Dalamud.Plugin.Services;
using Fleck;
using Newtonsoft.Json;
using NextUIOverlay.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;

#nullable enable
namespace NextUIOverlay.Socket;

public class NextUISocket : IDisposable
{
  protected WebSocketServer? server;
  protected readonly List<IWebSocketConnection> sockets = new List<IWebSocketConnection>();
  protected readonly Dictionary<string, List<IWebSocketConnection>> eventSubscriptions = new Dictionary<string, List<IWebSocketConnection>>();
  protected static Dictionary<string, Action<IWebSocketConnection, SocketEvent>> actions = new Dictionary<string, Action<IWebSocketConnection, SocketEvent>>();
  protected bool running;
  protected bool commandsRegistered;

  public int Port { get; set; }

  public NextUISocket(int port) => this.Port = port;

  protected static bool CheckIfPortAvailable(int portToCheck)
  {
    return !((IEnumerable<IPEndPoint>) IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners()).Select<IPEndPoint, int>((Func<IPEndPoint, int>) (p => p.Port)).ToList<int>().Contains(portToCheck);
  }

  public void Start()
  {
    if (!NextUISocket.CheckIfPortAvailable(this.Port))
    {
      this.running = false;
      IPluginLog pluginLog = NextUIPlugin.pluginLog;
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 1);
      interpolatedStringHandler.AppendLiteral("NUSocket is unable to launch server at port ");
      interpolatedStringHandler.AppendFormatted<int>(this.Port);
      string stringAndClear = interpolatedStringHandler.ToStringAndClear();
      object[] objArray = Array.Empty<object>();
      pluginLog.Warning(stringAndClear, objArray);
    }
    else
    {
      this.server = new WebSocketServer("ws://" + IPAddress.Loopback?.ToString() + ":" + this.Port.ToString() + "/ws");
      this.server.ListenerSocket.NoDelay = true;
      this.server.RestartAfterListenError = true;
      if (!this.commandsRegistered)
        this.commandsRegistered = true;
      this.server.Start((Action<IWebSocketConnection>) (socket =>
      {
        socket.OnOpen = (Action) (() => this.OpenSocket(socket));
        socket.OnClose = (Action) (() => this.CloseSocket(socket));
        socket.OnMessage = (Action<string>) (message => this.OnMessage(message, socket));
      }));
      this.running = true;
      IPluginLog pluginLog = NextUIPlugin.pluginLog;
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(32, 1);
      interpolatedStringHandler.AppendLiteral("NUSocket server started at port ");
      interpolatedStringHandler.AppendFormatted<int>(this.Port);
      string stringAndClear = interpolatedStringHandler.ToStringAndClear();
      object[] objArray = Array.Empty<object>();
      pluginLog.Information(stringAndClear, objArray);
    }
  }

  protected void OpenSocket(IWebSocketConnection socket) => this.sockets.Add(socket);

  protected void CloseSocket(IWebSocketConnection socket)
  {
    this.sockets.Remove(socket);
    foreach ((string _, List<IWebSocketConnection> socketConnectionList) in this.eventSubscriptions)
    {
      if (socketConnectionList.Contains(socket))
        socketConnectionList.Remove(socket);
    }
    try
    {
      socket.Close();
    }
    catch (Exception ex)
    {
      NextUIPlugin.pluginLog.Error(ex.Message, Array.Empty<object>());
    }
  }

  public bool IsRunning() => this.running;

  public static void RegisterCommand(
    string commandName,
    Action<IWebSocketConnection, SocketEvent> command)
  {
    if (NextUISocket.actions.ContainsKey(commandName))
      throw new Exception("Socket Command already registered: " + commandName);
    NextUISocket.actions.Add(commandName, command);
  }

  protected void OnMessage(string data, IWebSocketConnection socket)
  {
    SocketEvent ev1 = JsonConvert.DeserializeObject<SocketEvent>(data);
    if (ev1 == null)
      return;
    try
    {
      Action<IWebSocketConnection, SocketEvent> action;
      if (NextUISocket.actions.TryGetValue(ev1.type, out action))
      {
        action(socket, ev1);
      }
      else
      {
        string name = (("Xiv").ToString() + ev1.type[0].ToString().ToUpper() + ev1.type);
        MethodInfo method = this.GetType().GetMethod(name);
        if (method == (MethodInfo) null)
          NextUISocket.Respond(socket, ev1, (object) new
          {
            success = false,
            message = ("Unrecognized command: " + ev1.type)
          });
        else
          method.Invoke((object) this, new object[2]
          {
            (object) socket,
            (object) ev1
          });
      }
    }
    catch (Exception ex)
    {
      IWebSocketConnection socket1 = socket;
      SocketEvent ev2 = ev1;
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 2);
      interpolatedStringHandler.AppendLiteral("Unrecognized data: ");
      interpolatedStringHandler.AppendFormatted(data);
      interpolatedStringHandler.AppendLiteral(" ");
      interpolatedStringHandler.AppendFormatted<Exception>(ex);
      var data1 = new
      {
        success = false,
        message = interpolatedStringHandler.ToStringAndClear()
      };
      NextUISocket.Respond(socket1, ev2, (object) data1);
    }
  }

  public void XivSubscribeTo(IWebSocketConnection socket, SocketEvent ev)
  {
    string[] events = ev.request?.events;
    if (events == null || events.Length == 0)
    {
      NextUISocket.Respond(socket, ev, (object) new
      {
        success = false,
        message = "Invalid events"
      });
    }
    else
    {
      foreach (string key in events)
      {
        if (!this.eventSubscriptions.ContainsKey(key))
          this.eventSubscriptions.Add(key, new List<IWebSocketConnection>());
        if (!this.eventSubscriptions[key].Contains(socket))
          this.eventSubscriptions[key].Add(socket);
      }
      NextUISocket.Respond(socket, ev, (object) new
      {
        success = true,
        message = ("Subscribed to: " + string.Join(", ", events))
      });
    }
  }

  public void XivUnsubscribeFrom(IWebSocketConnection socket, SocketEvent ev)
  {
    string[] events = ev.request?.events;
    if (events == null || events.Length == 0)
    {
      NextUISocket.Respond(socket, ev, (object) new
      {
        success = false,
        message = "Invalid events"
      });
    }
    else
    {
      foreach (string key in events)
      {
        if (this.eventSubscriptions.ContainsKey(key) && this.eventSubscriptions[key].Contains(socket))
          this.eventSubscriptions[key].Remove(socket);
      }
      NextUISocket.Respond(socket, ev, (object) new
      {
        success = true,
        message = ("Unsubscribed from: " + string.Join(", ", events))
      });
    }
  }

  public List<IWebSocketConnection>? GetEventSubscriptions(string eventName)
  {
    List<IWebSocketConnection> eventSubscriptions;
    this.eventSubscriptions.TryGetValue(eventName, out eventSubscriptions);
    return eventSubscriptions;
  }

  public bool HasEventSubscriptions(string eventName)
  {
    List<IWebSocketConnection> eventSubscriptions = this.GetEventSubscriptions(eventName);
    return eventSubscriptions != null && eventSubscriptions.Count > 0;
  }

  public void XivSetAcceptFocus(IWebSocketConnection socket, SocketEvent ev)
  {
    string str = "AcceptFocus Changed " + ev.accept.ToString();
    foreach (OverlayGui overlay in NextUIPlugin.guiManager.overlays)
      overlay.acceptFocus = ev.accept;
    NextUISocket.Respond(socket, ev, (object) new
    {
      success = true,
      message = str
    });
    NextUIPlugin.pluginLog.Information(str, Array.Empty<object>());
  }

  public void Broadcast(string message)
  {
    this.sockets.ForEach((Action<IWebSocketConnection>) (s => s.Send(message)));
  }

  public void Broadcast(object message) => this.Broadcast(JsonConvert.SerializeObject(message));

  public static void BroadcastTo(object data, List<IWebSocketConnection> socketConnections)
  {
    foreach (IWebSocketConnection socketConnection in socketConnections)
      socketConnection.Send(JsonConvert.SerializeObject(data));
  }

  public static void Send(IWebSocketConnection socket, object message)
  {
    socket.Send(JsonConvert.SerializeObject(message));
  }

  public static void Respond(IWebSocketConnection socket, SocketEvent ev, object? data)
  {
    NextUISocket.Send(socket, (object) new
    {
      @event = ev.type,
      guid = ev.guid,
      data = data
    });
  }

  public void Restart()
  {
    NextUIPlugin.pluginLog.Information("Restarting websocket server", Array.Empty<object>());
    this.Stop();
    this.Start();
  }

  public void Stop()
  {
    try
    {
      this.Dispose();
    }
    catch (Exception ex)
    {
      NextUIPlugin.pluginLog.Information(ex.ToString(), Array.Empty<object>());
    }
  }

  public void Dispose()
  {
    this.sockets.ToList<IWebSocketConnection>().ForEach(new Action<IWebSocketConnection>(this.CloseSocket));
    this.server?.Dispose();
  }
}