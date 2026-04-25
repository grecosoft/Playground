package main

import (
	"context"
	"encoding/json"
	"fmt"
	"io"
	"log/slog"
	"net/http"
	"os"
	"os/signal"
	"strings"
	"syscall"

	"nhooyr.io/websocket"
)

func main() {
	address := "http://localhost:8087/connectorhub"

	negotiateURL := address + "/negotiate?negotiateVersion=1&agentIdentity=agent1"
	slog.Info("Negotiating", "url", negotiateURL)
	resp, err := http.Post(negotiateURL, "application/json", nil)

	slog.Info("Negotiated", "resp", resp, "err", err)

	// Step 1: Negotiate to get Azure SignalR Service URL and token
	wsURL, token, err := negotiate(address, "agent1")
	if err != nil {
		slog.Error("Negotiate failed", "error", err)
		os.Exit(1)
	}

	slog.Info("Negotiate successful", "url", wsURL)
	slog.Info(token)

	// Step 2: Connect WebSocket to Azure SignalR Service
	ctx := context.Background()
	conn, _, err := websocket.Dial(ctx, wsURL, &websocket.DialOptions{
		HTTPHeader: http.Header{
			"Authorization": []string{"Bearer " + token},
		},
	})
	if err != nil {
		slog.Error("WebSocket dial failed", "error", err)
		os.Exit(1)
	}
	defer conn.Close(websocket.StatusNormalClosure, "shutdown")
	slog.Info("WebSocket connected")

	// Step 3: Send SignalR JSON protocol handshake
	handshake := `{"protocol":"json","version":1}` + "\x1e"
	if err := conn.Write(ctx, websocket.MessageText, []byte(handshake)); err != nil {
		slog.Error("Handshake failed", "error", err)
		os.Exit(1)
	}

	// Step 4: Read handshake ack then start message loop
	_, data, err := conn.Read(ctx)
	if err != nil {
		slog.Error("Handshake ack failed", "error", err)
		os.Exit(1)
	}
	slog.Info("Handshake ack", "data", string(data))

	// Step 5: Message loop
	go func() {
		for {
			_, data, err := conn.Read(ctx)
			if err != nil {
				slog.Error("Read error", "error", err)
				return
			}

			raw := strings.TrimSuffix(string(data), "\x1e")
			if raw == "" || raw == "{}" {
				continue
			}

			var msg signalRMessage
			if err := json.Unmarshal([]byte(raw), &msg); err != nil {
				slog.Warn("Failed to parse message", "raw", raw)
				continue
			}

			switch msg.Type {
			case 1: // Invocation
				handleInvocation(ctx, conn, msg)
			case 6: // Ping — respond with pong
				pong := `{"type":6}` + "\x1e"
				_ = conn.Write(ctx, websocket.MessageText, []byte(pong))
			}
		}
	}()

	// Wait for shutdown
	quit := make(chan os.Signal, 1)
	signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)
	<-quit
	slog.Info("Shutting down")
}

type signalRMessage struct {
	Type         int             `json:"type"`
	InvocationId string          `json:"invocationId,omitempty"`
	Target       string          `json:"target,omitempty"`
	Arguments    json.RawMessage `json:"arguments,omitempty"`
	Error        string          `json:"error,omitempty"`
}

func negotiate(hubURL string, agentIdentity string) (string, string, error) {
	resp, err := http.Post(
		hubURL+"/negotiate?negotiateVersion=1&agentIdentity="+agentIdentity,
		"application/json",
		nil,
	)
	if err != nil {
		return "", "", fmt.Errorf("negotiate request: %w", err)
	}
	defer resp.Body.Close()

	// Print raw response first
	body, _ := io.ReadAll(resp.Body)
	slog.Info("Negotiate response", "body", string(body))

	var result struct {
		Url         string `json:"url"`
		AccessToken string `json:"accessToken"`
	}
	if err := json.Unmarshal(body, &result); err != nil {
		return "", "", fmt.Errorf("decode negotiate: %w", err)
	}

	return result.Url, result.AccessToken, nil
}

func handleInvocation(ctx context.Context, conn *websocket.Conn, msg signalRMessage) {
	slog.Info("Invocation received", "target", msg.Target)

	// Return result directly to the InvokeAsync caller
	// Return result directly to the InvokeAsync caller
	sendCompletion(ctx, conn, msg.InvocationId, map[string]any{
		"correlationId": "asfsadf",
		"status":        "Response from GoLang",
		"result":        "command processed",
	})

	//sendHubMessage(ctx, conn, "SendResponseToCommand", "agent.commands.ping", `{"status":"ok"}`)

	//switch msg.Target {
	//case "ReceiveCommand":
	//	var args []CommandMessage
	//	if err := json.Unmarshal(msg.Arguments, &args); err != nil {
	//		slog.Error("Failed to parse command arguments", "error", err)
	//		return
	//	}
	//	if len(args) > 0 {
	//		slog.Info("Command received",
	//			"correlationId", args[0].CorrelationId,
	//			"commandType", args[0].CommandType)
	//	}
	//
	//case "ReceiveMessage":
	//	var args []string
	//	if err := json.Unmarshal(msg.Arguments, &args); err != nil {
	//		slog.Error("Failed to parse message arguments", "error", err)
	//		return
	//	}
	//	if len(args) > 0 {
	//		slog.Info("Message received", "message", args[0])
	//	}
	//
	//default:
	//	slog.Warn("Unknown invocation target", "target", msg.Target)
	//}
}

func sendHubMessage(ctx context.Context, conn *websocket.Conn, method string, args ...any) {
	argsJSON, _ := json.Marshal(args)
	msg := map[string]any{
		"type":      1,
		"target":    method,
		"arguments": json.RawMessage(argsJSON),
	}
	data, _ := json.Marshal(msg)
	payload := string(data) + "\x1e"
	if err := conn.Write(ctx, websocket.MessageText, []byte(payload)); err != nil {
		slog.Error("Failed to send hub message", "error", err)
	}
}

func sendCompletion(ctx context.Context, conn *websocket.Conn, invocationId string, result any) {
	resultJSON, _ := json.Marshal(result)
	msg := map[string]any{
		"type":         3,
		"invocationId": invocationId,
		"result":       json.RawMessage(resultJSON),
	}
	data, _ := json.Marshal(msg)
	payload := string(data) + "\x1e"
	if err := conn.Write(ctx, websocket.MessageText, []byte(payload)); err != nil {
		slog.Error("Failed to send completion", "error", err)
	}
}
