using Fusion;

namespace GameLoop
{
    public class Objective
    {
        private ObjectiveData data;
        private int currVal;
        public string ObjectiveString => data.objectiveText;
        public int Current => currVal;
        public int Target => data.targetValue;
        public bool IsCompleted => currVal == data.targetValue;
        public ObjectiveData Data => data;
        
        public Objective(ObjectiveData data)
        {
            this.data = data;
            this.currVal = data.value;
        }

        public void SetValue(int value)
        {
            this.currVal = value;
        }
        
        public void UpdateObjective()
        {
            switch (data.operationType)
            {
                case ObjectiveData.OperationType.Add:
                    currVal += 1;
                    break;
                case ObjectiveData.OperationType.Sub:
                    currVal -= 1;
                    break;
            }
        }
    }
}