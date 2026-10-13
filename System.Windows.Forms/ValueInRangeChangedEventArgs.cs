namespace System.Windows.Forms;

public class ValueInRangeChangedEventArgs : EventArgs
{
	public AGaugeRange Range { get; private set; }

	public float Value { get; private set; }

	public bool InRange { get; private set; }

	public ValueInRangeChangedEventArgs(AGaugeRange range, float value, bool inRange)
	{
		Range = range;
		Value = value;
		InRange = inRange;
	}
}
