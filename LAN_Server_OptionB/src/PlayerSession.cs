using System.Collections;
using System.Net.Sockets;

namespace CNRLanServer;

/// <summary>
/// State for one connected Photon client (TCP connection).
/// Handles the receive-buffer accumulation logic and owns the TcpClient.
/// </summary>
public class PlayerSession : IDisposable
{
    private static int _idCounter = 0;

    // ─── Identity ────────────────────────────────────────────────────────
    public int    Id          { get; } = Interlocked.Increment(ref _idCounter);
    public string UserId      { get; set; } = "";
    public string Nickname    { get; set; } = "";
    public bool   IsAuthenticated { get; set; }

    // ─── Room state (game server only) ───────────────────────────────────
    public RoomInfo? CurrentRoom    { get; set; }
    public int       ActorNr        { get; set; }
    public Hashtable PlayerProps    { get; } = new();

    // ─── TCP plumbing ────────────────────────────────────────────────────
    public TcpClient     Client  { get; }
    public NetworkStream Stream  => Client.GetStream();
    public bool          IsAlive => Client.Connected;

    // Receive ring-buffer — we accumulate bytes here until a full frame is ready
    private readonly byte[] _recvBuf = new byte[65536];
    private int _recvLen;

    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private bool _disposed;

    public PlayerSession(TcpClient client)
    {
        Client = client;
        Client.NoDelay = true;
        Client.ReceiveTimeout = 0; // async, no timeout
    }

    // ─── Send ──────────────────────────────────────────────────────────

    public async Task SendAsync(byte[] frame, CancellationToken ct = default)
    {
        if (!IsAlive) return;
        await _sendLock.WaitAsync(ct);
        try
        {
            await Stream.WriteAsync(frame, ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Session {Id}] Send error: {ex.Message}");
        }
        finally
        {
            _sendLock.Release();
        }
    }

    // ─── Receive accumulation ──────────────────────────────────────────

    /// <summary>
    /// Appends newly received bytes and yields each complete frame.
    /// Call this in a loop from the connection reader task.
    /// </summary>
    public IEnumerable<byte[]> AppendAndExtract(byte[] data, int len)
    {
        // Grow if necessary
        if (_recvLen + len > _recvBuf.Length)
        {
            // Drop — protocol error (shouldn't happen with normal game traffic)
            Console.WriteLine($"[Session {Id}] Receive buffer overflow, resetting.");
            _recvLen = 0;
            yield break;
        }

        Buffer.BlockCopy(data, 0, _recvBuf, _recvLen, len);
        _recvLen += len;

        while (true)
        {
            int frameSize = PhotonBinaryProtocol.PeekFrameSize(_recvBuf, _recvLen);
            if (frameSize <= 0)
                yield break;

            byte[] frame = new byte[frameSize];
            Buffer.BlockCopy(_recvBuf, 0, frame, 0, frameSize);

            // Shift remaining bytes
            _recvLen -= frameSize;
            if (_recvLen > 0)
                Buffer.BlockCopy(_recvBuf, frameSize, _recvBuf, 0, _recvLen);

            yield return frame;
        }
    }

    // ─── Dispose ──────────────────────────────────────────────────────

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { Client.Close(); } catch { /* */ }
        _sendLock.Dispose();
    }
}
