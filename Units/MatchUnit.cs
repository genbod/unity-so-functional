using Ludiq;
using Bolt;
using System;
using static F;
using System.Reflection;

public class MatchUnit : Unit, ISelectUnit
{
    [SerializeAs("Type")]
    private Type _optionType;
    [DoNotSerialize]
    [Inspectable]
    [UnitHeaderInspectable("Option Type")]
    public Type optionType
    {
        get { return _optionType;}
        set { _optionType = value;}
    }
    
    [DoNotSerialize]
    public ValueInput optionInput { get; private set; }

    [DoNotSerialize]
    public ValueInput noneInput { get; private set; }

    /// <summary>
    /// The returned value.
    /// </summary>
    [DoNotSerialize]
    [PortLabelHidden]
    public ValueOutput selection { get; private set; }

    protected override void Definition()
    {
		optionInput = ValueInput(typeof(Option<System.Object>), nameof(optionInput));
        noneInput = ValueInput(optionType, nameof(noneInput));
        selection = ValueOutput(optionType, nameof(selection), Branch).Predictable();
    }

    public object Branch(Recursion recursion)
    {
        Option<System.Object> castOption = None;
        var option = (Option<System.Object>)optionInput.GetValue();
        
		var result = option.Match(
			() => noneInput.GetValue(optionType),
			(f) => f
		);

        return result.ConvertTo(optionType);
    }
}