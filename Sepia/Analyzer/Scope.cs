namespace Sepia.Analyzer;

public class Scope : IDisposable
{
    private bool disposed;
    private Stack<Dictionary<string, ScopeInfo>> scopes;

    public Scope(Stack<Dictionary<string, ScopeInfo>> scopes)
    {
        this.scopes = scopes;
        scopes.Push(new());
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                scopes.Pop();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}