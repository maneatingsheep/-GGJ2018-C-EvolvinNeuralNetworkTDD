public class LogicTile {


    public enum ActionType {None, Grow, New };
    
    public int NumValue = 0;
    public ActionType PossibleAction = ActionType.None;
    internal bool Merged = false;

    public void Finlize() {
        Merged = false;
        
        PossibleAction = ActionType.None;
    }

 

}
