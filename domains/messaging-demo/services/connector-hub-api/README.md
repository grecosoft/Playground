# Messaging Hub

## What is this?

The Messaging Hub is a **translation layer** that sits between two very different worlds:

- **Internal services** — back-end services that talk to each other over a message bus (Azure Service Bus). These services are always-on, trusted, and live inside the platform. *Demo One* and *Demo Two* are examples.
- **External agents** — software running outside the platform, often on customer premises or on remote devices. These agents connect over a persistent real-time connection (SignalR) because they may be behind firewalls or NAT and cannot be reached directly. *Acme Agent* is an example.

Without the Messaging Hub, internal services would have no way to send work to those external agents, and agents would have no way to receive instructions from the platform.

---

## The Big Picture

```
╔══════════════════════════════════════════════════════════════════╗
║                     Internal Platform                            ║
║                                                                  ║
║   ┌──────────────┐          ┌──────────────┐                    ║
║   │  Demo One    │          │  Demo Two    │   ... other         ║
║   │  (internal)  │          │  (internal)  │       services      ║
║   └──────┬───────┘          └──────┬───────┘                    ║
║          │                         │                             ║
║          └──────────┬──────────────┘                            ║
║                     │  Azure Service Bus                         ║
║                     ▼                                            ║
║          ┌──────────────────────┐                               ║
║          │    Messaging Hub     │  ◄─── You are here            ║
║          └──────────┬───────────┘                               ║
╚═════════════════════╪════════════════════════════════════════════╝
                      │  Azure SignalR Service
          ════════════╪═════════════════ Internet boundary
                      │
         ┌────────────┴────────────────────┐
         │                                 │
  ┌──────▼───────┐                 ┌───────▼──────┐
  │  Acme Agent  │                 │  Other Agent │
  │  (external)  │                 │  (external)  │
  └──────────────┘                 └──────────────┘
```

---

## How it works — day to day

1. **Agents connect and stay connected.**  
   When an agent starts up, it opens a persistent connection to the Messaging Hub and keeps it open. The hub knows which agent is on which connection at any given moment.

2. **Internal services send requests over the message bus.**  
   An internal service (e.g. Demo One) puts a request on the Service Bus — for example, "ping agent1" or "get a status report from agent2". It does not need to know anything about SignalR or where the agent is.

3. **The hub picks up the request and forwards it to the right agent.**  
   The hub receives the request from the bus, looks up which live connection belongs to the target agent, and pushes the request down that connection in real time.

4. **The agent responds (when required).**  
   - For a **ping** (needs an instant answer): the agent replies immediately and the hub sends the answer straight back to the internal service — the whole exchange looks like a normal request/response.  
   - For a **status report** (longer running work): the hub saves a record of the request and fires it off to the agent without waiting. The agent can do the work in its own time and report back later.

---

## The two types of request

| Type | Example | What happens |
|---|---|---|
| **Request / Reply** | Ping an agent | Hub asks the agent, waits for the answer, returns it immediately to the caller |
| **Fire and Forget** | Ask for a status report | Hub saves the request, delivers it to the agent, and returns immediately — the result comes back asynchronously |

---

## Ping Command — end-to-end flow

This diagram shows exactly what happens when *Demo One* sends a ping to *Acme Agent* via the Messaging Hub.

```
 Demo One (internal)          Messaging Hub               Acme Agent (external)
        │                           │                              │
        │  ① POST /company/{id}/    │                              │
        │    {agentId}/ping         │                              │
        │                           │                              │
        │──② AgentPingCommand ─────►│                              │
        │    (Service Bus)          │                              │
        │                           │──③ Look up agent's ──────────│
        │                           │   live connection            │
        │                           │                              │
        │                           │──④ Forward ping ────────────►│
        │                           │   (SignalR, real-time)       │
        │                           │                              │
        │                           │◄─⑤ AgentPingResponse ────────│
        │                           │   (SignalR, same connection) │
        │                           │                              │
        │◄─⑥ Response ──────────────│                              │
        │   (Service Bus reply)     │                              │
        │                           │                              │
        ▼                           ▼                              ▼
```

### Step-by-step

| Step | Who | What |
|---|---|---|
| ① | Demo One | An HTTP endpoint receives a ping request for a specific company and agent |
| ② | Demo One → Hub | The ping command is published to the Azure Service Bus and picked up by the Messaging Hub |
| ③ | Messaging Hub | The hub looks up the active real-time connection for the target agent |
| ④ | Hub → Acme Agent | The ping is pushed over the live SignalR connection to the agent |
| ⑤ | Acme Agent → Hub | The agent processes the ping and returns a response on the same connection |
| ⑥ | Hub → Demo One | The hub sends the agent's response back to Demo One via the Service Bus reply channel |

---

## Known limitations (POC)

- **Agent identity is not secured.** Agents currently identify themselves via a query-string parameter when they connect. In production this should be replaced with a proper signed token issued by an authentication service.


