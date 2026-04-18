import { useState, useRef, useEffect } from 'react';

const BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5100';

interface Message {
  id: string;
  role: 'user' | 'assistant' | 'tool';
  content: string;
  toolCalls?: ToolCall[];
  toolCallId?: string;
}

interface ToolCall {
  id: string;
  type: 'function';
  function: { name: string; arguments: string };
}

interface ToolCallInfo {
  id: string;
  name: string;
  args: string;
  result?: string;
}

export default function Agent() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState('');
  const [running, setRunning] = useState(false);
  const [error, setError] = useState('');
  const [toolCalls, setToolCalls] = useState<ToolCallInfo[]>([]);
  const threadIdRef = useRef<string>(
    // Use hyphen-free UUID to match the server's Guid.NewGuid().ToString("N") format
    crypto.randomUUID().replace(/-/g, '')
  );
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, toolCalls]);

  const sendMessage = async () => {
    const text = input.trim();
    if (!text || running) return;

    const userMsg: Message = { id: crypto.randomUUID(), role: 'user', content: text };
    const updatedMessages = [...messages, userMsg];
    setMessages(updatedMessages);
    setInput('');
    setRunning(true);
    setError('');

    const runId = crypto.randomUUID().replace(/-/g, '');
    let assistantMsgId: string | null = null;
    let assistantText = '';
    const pendingToolCalls: ToolCallInfo[] = [];

    try {
      const response = await fetch(`${BASE_URL}/agent`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          threadId: threadIdRef.current,
          runId,
          messages: updatedMessages.map(m => ({
            id: m.id,
            role: m.role,
            content: m.content || null,
            toolCalls: m.toolCalls,
            toolCallId: m.toolCallId,
          })),
        }),
      });

      if (!response.ok || !response.body) {
        throw new Error(`HTTP ${response.status}`);
      }

      const reader = response.body.getReader();
      const decoder = new TextDecoder();
      let buffer = '';

      while (true) {
        const { value, done } = await reader.read();
        if (done) break;
        buffer += decoder.decode(value, { stream: true });

        const lines = buffer.split('\n');
        buffer = lines.pop() ?? '';

        for (const line of lines) {
          if (!line.startsWith('data: ')) continue;
          const json = line.slice(6).trim();
          if (!json) continue;

          let event: Record<string, string>;
          try { event = JSON.parse(json); } catch { continue; }

          switch (event.type) {
            case 'TEXT_MESSAGE_START':
              assistantMsgId = event.messageId;
              assistantText = '';
              setMessages(prev => [...prev, { id: assistantMsgId!, role: 'assistant', content: '' }]);
              break;

            case 'TEXT_MESSAGE_CONTENT':
              assistantText += event.delta;
              setMessages(prev => prev.map(m =>
                m.id === assistantMsgId ? { ...m, content: assistantText } : m
              ));
              break;

            case 'TEXT_MESSAGE_END':
              assistantMsgId = null;
              break;

            case 'TOOL_CALL_START':
              pendingToolCalls.push({ id: event.toolCallId, name: event.toolCallName, args: '' });
              setToolCalls(prev => [...prev, { id: event.toolCallId, name: event.toolCallName, args: '' }]);
              break;

            case 'TOOL_CALL_ARGS': {
              const tc = pendingToolCalls.find(t => t.id === event.toolCallId);
              if (tc) tc.args += event.delta;
              setToolCalls(prev => prev.map(t =>
                t.id === event.toolCallId ? { ...t, args: t.args + event.delta } : t
              ));
              break;
            }

            case 'TOOL_CALL_RESULT':
              setToolCalls(prev => prev.map(t =>
                t.id === event.toolCallId ? { ...t, result: event.content } : t
              ));
              break;

            case 'RUN_ERROR':
              setError(event.message ?? 'An error occurred');
              break;
          }
        }
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
    } finally {
      setRunning(false);
    }
  };

  const clearChat = () => {
    setMessages([]);
    setToolCalls([]);
    setError('');
    threadIdRef.current = crypto.randomUUID().replace(/-/g, '');
  };

  return (
    <div className="agent-page">
      <div className="agent-header">
        <h1 className="page-title">🤖 Chinook AI Assistant</h1>
        <button className="btn-secondary" onClick={clearChat} disabled={running}>New chat</button>
      </div>
      <p className="agent-description">
        Ask about artists, albums, tracks, playlists, customers, or genres in the Chinook music database.
      </p>

      <div className="agent-layout">
        <div className="chat-panel">
          <div className="chat-messages">
            {messages.length === 0 && (
              <div className="chat-empty">
                <p>Try: <em>"Who are the top 5 artists by number of albums?"</em></p>
                <p>Or: <em>"Show me all albums by AC/DC"</em></p>
                <p>Or: <em>"What genres are available?"</em></p>
              </div>
            )}
            {messages.map(m => (
              <div key={m.id} className={`chat-message chat-message--${m.role}`}>
                <span className="chat-role">{m.role === 'user' ? 'You' : 'Assistant'}</span>
                <p className="chat-content">{m.content}</p>
              </div>
            ))}
            {running && (() => {
              const isWaitingForResponse = !messages.some(m => m.role === 'assistant' && m.content === '');
              return isWaitingForResponse ? (
                <div className="chat-message chat-message--assistant">
                  <span className="chat-role">Assistant</span>
                  <p className="chat-thinking">Thinking…</p>
                </div>
              ) : null;
            })()}
            {error && <div className="chat-error">{error}</div>}
            <div ref={messagesEndRef} />
          </div>

          <div className="chat-input-row">
            <input
              className="chat-input"
              value={input}
              onChange={e => setInput(e.target.value)}
              onKeyDown={e => e.key === 'Enter' && !e.shiftKey && sendMessage()}
              placeholder="Ask about the music catalog…"
              disabled={running}
            />
            <button className="btn-primary" onClick={sendMessage} disabled={running || !input.trim()}>
              {running ? '…' : 'Send'}
            </button>
          </div>
        </div>

        {toolCalls.length > 0 && (
          <div className="tool-panel">
            <h3 className="tool-panel-title">Tool calls</h3>
            {toolCalls.map(tc => (
              <div key={tc.id} className="tool-call-card">
                <div className="tool-call-name">🔧 {tc.name}</div>
                {tc.args && (
                  <pre className="tool-call-args">{tc.args}</pre>
                )}
                {tc.result !== undefined && (
                  <pre className="tool-call-result">{tc.result}</pre>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
