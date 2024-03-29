﻿using System;

namespace ExternPropertyAttributes
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class DisableIfAttribute : EnableIfAttributeBase
	{
		public DisableIfAttribute(string condition)
			: base(condition)
		{
			Inverted = true;
		}

		public DisableIfAttribute(EConditionOperator conditionOperator, params string[] conditions)
			: base(conditionOperator, conditions)
		{
			Inverted = true;
		}
	}
}
