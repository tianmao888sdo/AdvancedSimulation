
public abstract class AbsScene
{
    protected int m_id;

    public AbsScene(int id)
    {
        m_id = id;
    }

    protected virtual void EnterScene() { }

    public virtual void ExitScene() { }

    public virtual void StartLoading()
    {
    }
}