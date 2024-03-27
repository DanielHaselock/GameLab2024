namespace GameLoop
{
    public class Objective
    {
        private ObjectiveData data;
        private int currVal=0;

        public bool IsCompleted => currVal == data.targetValue;
        
        public Objective(ObjectiveData data)
        {
            this.data = data;
            this.currVal = data.value;
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