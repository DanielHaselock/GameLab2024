﻿using System;

namespace ExternPropertyAttributes
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class EnableIfAttribute : EnableIfAttributeBase
	{
		public EnableIfAttribute(string condition)
			: base(condition)
		{
			Inverted = false;
		}

		public EnableIfAttribute(EConditionOperator conditionOperator, params string[] conditions)
			: base(conditionOperator, conditions)
		{
			Inverted = false;
		}
	}
}
