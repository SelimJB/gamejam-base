using System;

[Serializable]
public struct SerializableNullable<T> where T : struct
{
	public bool hasValue;
	public T value;

	public T? Value
	{
		get => hasValue ? value : null;
		set
		{
			if (value.HasValue)
			{
				hasValue = true;
				this.value = value.Value;
			}
			else
			{
				hasValue = false;
				this.value = default;
			}
		}
	}
}