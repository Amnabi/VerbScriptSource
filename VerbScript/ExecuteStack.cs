using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace VerbScript {
	public static class ContextHelper{
		public static bool quickEvaluateAsBool(this VerbSequence vs, ExecuteStackContext esc){
			esc.pushReturnType(ReturnTypeStack.Default);
			IEnumerable<object> iob = quickEvaluate(vs, esc);
			object obj = iob.singular();
			esc.popReturnType();
			if(obj is int i){
				throw new Exception("Int type no longer supported");
				//return i == vs.uniqueSubIDFromContent();
			}else if(obj is bool b){
				return b;
			}
			throw new Exception("Unknown type");
		}
		public static IEnumerable<int> quickEvaluateAsCT(this VerbSequence vs, ExecuteStackContext esc){
			esc.pushReturnType(ReturnTypeStack.INTRAW);
			IEnumerable<object> iob = quickEvaluate(vs, esc);
			List<object> objLS = FreePool<List<object>>.next();
			objLS.AddRange(iob);
			esc.popReturnType();
			foreach(object c in objLS){
				if(c is int i){
					yield return i;
				}else if(c is bool b){
					if(b){
						yield return vs.uniqueSubIDFromContent();
					}else{
					}
				}else{
					throw new Exception("Unknown type");
				}
			}
			objLS.Clear();
			FreePool<List<object>>.free(objLS);
		}
		public static IEnumerable<object> quickEvaluate(this VerbSequence vs, ExecuteStackContext esc){
			if(VerbSequence.SLResultCache){
				IEnumerable<object> cach = vs.cachedResult;
				if(cach != null){
					return func4472874_NOVAS(vs, esc);
				}else{
					return func4472873_NOVAS(vs, esc);
				}
			}else{
				return func4472872(vs, esc);
			}
		}
		/**public static IEnumerable<object> quickEvaluateVAS(this VerbSequence vs, ExecuteStackContext esc){
			if(VerbSequence.SLResultCache){
				IEnumerable<object> cach = vs.cachedResult;
				if(cach != null){
					return func7662617(vs, esc);
				}else{
					return func7662616(vs, esc);
				}
			}else{
				return func7662615(vs, esc);
			}
		}**/
		/**
		//return NON VAS
		public static IEnumerable<object> func4472872(this VerbSequence vs, ExecuteStackContext esc){
			foreach(object obj in esc.quickEvaluate(vs)){
				yield return obj;
			}
			yield break;
		}
		//create cache, return NON VAS
		public static IEnumerable<object> func4472873(this VerbSequence vs, ExecuteStackContext esc){
			ICollection<object> objIC = vs.CreateResultCacheCollection();
			foreach(object obj in esc.quickEvaluate(vs)){
				ValueAndSource vas = new ValueAndSource(obj, vs);
				objIC.Add(vas);
				yield return obj;
			}
			vs.cachedResult = objIC;
		}
		//return NON-VAS cache
		public static IEnumerable<object> func4472874(this VerbSequence vs, ExecuteStackContext esc){
			foreach(ValueAndSource vas in vs.cachedResult){
				yield return vas.value;
			}
		}**/

		
		//return NON VAS
		public static IEnumerable<object> func4472872(this VerbSequence vs, ExecuteStackContext esc){
			foreach(object obj in esc.quickEvaluate(vs)){
				yield return obj;
			}
			yield break;
		}
		//create cache, return NON VAS
		public static IEnumerable<object> func4472873_NOVAS(this VerbSequence vs, ExecuteStackContext esc){
			ICollection<object> objIC = vs.CreateResultCacheCollection();
			foreach(object obj in esc.quickEvaluate(vs)){
				objIC.Add(obj);
				yield return obj;
			}
			vs.cachedResult = objIC;
		}
		//return NON-VAS cache
		public static IEnumerable<object> func4472874_NOVAS(this VerbSequence vs, ExecuteStackContext esc){
			foreach(object vas in vs.cachedResult){
				yield return vas;
			}
		}
		/**
		//create cache, return VAS
		public static IEnumerable<object> func7662616(this VerbSequence vs, ExecuteStackContext esc){
			ICollection<object> objIC = vs.CreateResultCacheCollection();
			foreach(object obj in esc.quickEvaluate(vs)){
				ValueAndSource vas = new ValueAndSource(obj, vs);
				objIC.Add(vas);
				yield return vas;
			}
			vs.cachedResult = objIC;
		}**/
		/**
		//return VAS
		public static IEnumerable<object> func7662614(this VerbSequence vs, ExecuteStackContext esc){
			foreach(object obj in esc.quickEvaluateVAS(vs)){
				yield return obj;
			}
			yield break;
		}
		//return VAS cache
		public static IEnumerable<object> func7662617(this VerbSequence vs, ExecuteStackContext esc){
			return vs.cachedResult;
		}**/

	}
	public class ExecuteStackContext : IExposable{
		public void ExposeData(){
			Scribe_Deep.Look<VariableHolder>(ref this.localVariableHolder, "LocalVariableHolder");
			Scribe_TargetInfo.Look(ref this.Param_TargetInfo, "Param_TargetInfo");
			if(Scribe.mode == LoadSaveMode.Saving){
				loadStateExeStack.Clear();
				loadSubSequenceGroupExeStack.Clear();
				foreach(ExecuteStack eS in executeStack){
					loadStateExeStack.Add(eS.stackIndex | (eS.stackExecuted? BF_Completed : 0));
					loadSubSequenceGroupExeStack.Add(eS.subSequenceGroupIndex);
				}
				Scribe_Values.Look<string>(ref this.verbScript.scriptName, "scriptName");
				//logTest();
			}
			if(Scribe.mode == LoadSaveMode.LoadingVars){
				string nameOut = null;
				Scribe_Values.Look<string>(ref nameOut, "scriptName");
				
				if(VerbScript.scriptByName.TryGetValue(nameOut, out VerbScript vs)){
					this.verbScript = vs;
				}else{
					Log.Error("Failed to load script!");
				}

			}
			Scribe_Deep.Look<LocalTargetInfoListed>(ref Param_ExtraTargetInfo, "Param_ExtraTargetInfo");
			Scribe_Values.Look<int>(ref this.delayTicks, "delayTicks");
			Scribe_Values.Look<bool>(ref this.complete, "complete");
			Scribe_Collections.Look<ReturnTypeStack>(ref this.returnTypeStack, "returnTypeStack");
			Scribe_Collections.Look<int>(ref loadSubSequenceGroupExeStack, "loadSubSequenceGroupExeStack");
			Scribe_Collections.Look<int>(ref loadStateExeStack, "loadStateExeStackIndex");
			Scribe_Collections.Look<int>(ref scopePointerStack, "scopePointerStack");
			if(this.flagDestroy.Count > 0){
				Log.Warning("FlagDestroy count is bigger than 0, they will not be saved or loaded.");
			}
		}
		public void logTest(){
			Log.Warning("Local");
			this.localVariableHolder.logTest();
			Log.Warning("Thing");
			this.thingVariableHolder.logTest();
			Log.Warning("delayTicks " + delayTicks);
			Log.Warning("complete " + complete);
			Log.Warning("Param_TargetInfo " + Param_TargetInfo);
			Log.Warning("scopePointerStack");
			for(int i = 0; i < scopePointerStack.Count; i++){
				Log.Warning(scopePointerStack[i] + "");
			}
			Log.Warning("executeStack");
			for(int i = 0; i < executeStack.Count; i++){
				executeStack[i].logTest();
			}
			Log.Warning("verbScript " + this.verbScript);
		}

		public const string SCOPESLPREFIX = "BE1AAF0F_ScopeSLPrefix";
		public static ExecuteStackContext SA_StaticContext = new ExecuteStackContext();
		public static List<ExecuteStackContext> free_ScriptExecutorContext = new List<ExecuteStackContext>();
		public static ExecuteStackContext nextScriptExecutorContext(){
			if(free_ScriptExecutorContext.Count > 0){
				return free_ScriptExecutorContext.Pop();
			}
			return new ExecuteStackContext();
		}
		public void clear(){
			complete = false;
			verbScript = null;
			localVariableHolder.clear();
			thingVariableHolder = null;
			executeStack.Clear();
			scopePointerStack.Clear();
			scopeStack.Clear();
			loadStateExeStack.Clear();
			loadSubSequenceGroupExeStack.Clear();
			Param_TargetInfo = default(LocalTargetInfo);
			PRIVATE_rootScope = null;
		}
		public void free(){
			clear();
			loadStateExeStack.Clear();
			loadSubSequenceGroupExeStack.Clear();
			free_ScriptExecutorContext.Add(this);
		}

		public LocalTargetInfo Param_TargetInfo;
		public LocalTargetInfoListed Param_ExtraTargetInfo;

		public bool complete = false;
		public VerbScript verbScript;
		public VariableHolder variableHolder(VariableHolderType vht){
			switch(vht){
				case VariableHolderType.TempScript:{
					return localVariableHolder;
				}
				case VariableHolderType.Local:{
					return thingVariableHolder;
				}
			}
			throw new Exception();
		}
		public VariableHolder localVariableHolder = new VariableHolder();
		public VariableHolder thingVariableHolder;
		public List<int> loadStateExeStack = new List<int>();
		public List<int> loadSubSequenceGroupExeStack = new List<int>();
		public List<ExecuteStack> executeStack = new List<ExecuteStack>();

		public const int BF_Completed = -0x80000000;
		private List<ReturnTypeStack> returnTypeStack = new List<ReturnTypeStack>();
		public void pushReturnType(ReturnTypeStack rts){
			returnTypeStack.Add(rts);
		}
		public void popReturnType(){
			returnTypeStack.Pop();
		}
		public ReturnTypeStack ReturnType(){
			if(returnTypeStack.Count > 0){
				return returnTypeStack[returnTypeStack.Count - 1];
			}
			return ReturnTypeStack.Default;
		}

		public List<int> scopePointerStack = new List<int>();
		public List<object> scopeStack = new List<object>();
		private object PRIVATE_rootScope; //not saved;
		public object rootScope{
			get{
				return PRIVATE_rootScope;
			}
			set{
				if(PRIVATE_rootScope != value){
					PRIVATE_rootScope = value;
					notify_PostScopeStackChange();
				}
			}
		}
		public object scopeNow;
		public void notify_PostScopeStackChange(){
			scopeNow = scope(scopeStack.Count - 1);
		}

		public int delayTicks = 0;
		public void clearStack(){
			executeStack.Clear();
		}
		public object scope(int i){
			if(i == -1){
				return rootScope;
			}else{
				return scopeStack[i];
			}
		}

		public static Dictionary<object, int> STACKPUSHREFNUM = new Dictionary<object, int>();
		public HashSet<object> flagDestroy = new HashSet<object>();
		public Dictionary<int, HashSet<object>> destroySafe = new Dictionary<int, HashSet<object>>();
		public void tryDestroy(object obj){
			if(STACKPUSHREFNUM.ContainsKey(obj)){
				flagDestroy.Add(obj);
			}else{
                if(obj is Thing thing){
                    thing.Destroy();
                }else if(obj is Hediff hd){
					hd.pawn.health.RemoveHediff(hd);
                }
			}

		}

		public static bool ShouldRecord(object obj){
			if(obj is Thing || obj is Hediff){
				return true;
			}
			return false;
		}

		public void pushScopeNoPointer(object scope){
			if(ShouldRecord(scope)){
				STACKPUSHREFNUM.setSafe(scope, STACKPUSHREFNUM.getSafe(scope) + 1);
			}
			scopeStack.Add(scope);
			notify_PostScopeStackChange();
		}
        public Func<HashSet<object>> SA_324739 = delegate(){ return new HashSet<object>(); };
		public void popScopeNoPointer(){
			object scope = scopeStack.Pop();
			if(ShouldRecord(scope)){
				int i = STACKPUSHREFNUM.getSafe(scope) - 1;
				STACKPUSHREFNUM.setSafe(scope, i);
				if(i == 0){
					if(flagDestroy.Contains(scope)){
						int cCount = executeStack.Count();
						destroySafe.getInitSafe(cCount, SA_324739).Add(scope);
						flagDestroy.Remove(scope);
					}
				}
			}
			notify_PostScopeStackChange();
		}

		public void pushScope(object scope){
			scopePointerStack.Add(executeStack.Count - 1);
			scopeStack.Add(scope);
			if(ShouldRecord(scope)){
				STACKPUSHREFNUM.setSafe(scope, STACKPUSHREFNUM.getSafe(scope) + 1);
			}
			//Log.Warning("Pop " + (scopePointerStack.Count - 1));
			if(scope is Thing t){
				this.localVariableHolder.saveVariable(SCOPESLPREFIX + (scopePointerStack.Count - 1), scope, VariableType.Thing);
			}else if(scope is Hediff h){
				this.localVariableHolder.saveVariable(SCOPESLPREFIX + (scopePointerStack.Count - 1), scope, VariableType.Hediff);
			}else{
				Log.Error("Unknown scope type, may cause errors on reloading " + scope);
			}

			notify_PostScopeStackChange();
		}

		public void pushExecuteStack(ExecuteStack stack){
			executeStack.Add(stack);
		}
		public void popExecuteStack(bool free = true){
			int sizeInd = executeStack.Count - 1;
			while(true){
				if(scopePointerStack.Count > 0){
					int sc = scopePointerStack.Count - 1;
					if(sizeInd == scopePointerStack[sc]){
						scopePointerStack.Pop();
						object scope = scopeStack.Pop();
						
						if(ShouldRecord(scope)){
							int i = STACKPUSHREFNUM.getSafe(scope) - 1;
							STACKPUSHREFNUM.setSafe(scope, i);
							if(i == 0){
								if(flagDestroy.Contains(scope)){
									int cCount = executeStack.Count();
									destroySafe.getInitSafe(cCount, FreePool<HashSet<object>>.next).Add(scope);
									flagDestroy.Remove(scope);
								}
							}
						}
						this.localVariableHolder.removeVariable((SCOPESLPREFIX + sc).ToString());
						notify_PostScopeStackChange();
					}else{
						break;
					}
				}else{
					break;
				}
			}
			if(free){
				executeStack.Pop().free();
			}else{
				executeStack.Pop();
			}
			int sizeInd2 = sizeInd + 1;
			if(destroySafe.TryGetValue(sizeInd2, out HashSet<object> objO)){
				foreach(object obj in objO){
					if(obj is Thing td){
						td.Destroy();
					}else if(obj is Hediff hd){
						hd.pawn.health.RemoveHediff(hd);
					}
				}
				objO.Clear();
				FreePool<HashSet<object>>.free(objO);
				destroySafe.Remove(sizeInd2);
			}

			/**if(destroySafe.Count() > 0){
				foreach(object obj in destroySafe){
					if(obj is Thing td){
						td.Destroy();
					}else if(obj is Hediff hd){
						hd.pawn.health.RemoveHediff(hd);
					}
				}
				destroySafe.Clear();
			}**/

		}
		public ExecuteStack topExecuteStack(){
			return executeStack[executeStack.Count - 1];
		}

		public IEnumerable<int> batchScopeExecute(List<VerbScope> scopeLeft, VerbSequence retSeq){
			for(int i = 0; i < scopeLeft.Count; i ++){
				ExecuteStack nextGen = ExecuteStack.nextScriptExecutor();
				nextGen.verbSequence = scopeLeft[i];
				nextGen.stackIndex = 0;
				pushExecuteStack(nextGen);
				nextGen.tryExecute(this);
			}
			IEnumerable<int> evaluateOut = retSeq.quickEvaluateAsCT(this);//retSeq.evaluate(this);
			for(int i = 0; i < scopeLeft.Count; i ++){
				this.popExecuteStack();
			}
			return evaluateOut;
		}
		//public static int calledTooMuch = 0;
		public IEnumerable<object> quickEvaluate(VerbSequence vs){//used by sub effect scopes, does not trigger tryPopWithChecks
			//throw new Exception();
			int ias = executeStack.Count;
			ExecuteStack nextGen = ExecuteStack.nextScriptExecutor();
			nextGen.verbSequence = vs;
			nextGen.stackIndex = 0;
			pushExecuteStack(nextGen);
			/**bool nope = true;
			if(SA_0 > SA_1 + 100){
				foreach(VerbSequence ve in SA_DesyncCheck0.Keys){
					Log.Message("KEV " + SA_DesyncCheck1.getSafe(ve) + " / " + SA_DesyncCheck0[ve] + " " + ve.uniqueID);
				}
				Log.Message("DESYNC " + SA_DesyncCheck0.Count + " " + SA_0 + " " + SA_1 + " " + SA_2);
			} 
			SA_DesyncCheck0.setSafe(vs, SA_DesyncCheck0.getSafe(vs) + 1);
			SA_0 += 1;**/
			while(ias != executeStack.Count){
				foreach(object obj in func389813(ias)){
					yield return obj;
					/**if(nope){
						nope = false;
						SA_2 += 1;
					}**/
				}
				//Log.Warning("Suspect F " + Rand.Value + " / " + new System.Diagnostics.StackTrace());
			}
			//SA_DesyncCheck1.setSafe(vs, SA_DesyncCheck1.getSafe(vs) + 1);
			//SA_1 += 1;
			yield break;
			//IEnumerable<object> evaluateOut = vs.evaluate(this);
			//this.popExecuteStack();
			//return evaluateOut;
		}
		public bool tryPopWithChecks(int popBreakIndex, bool force = false){
			ExecuteStack topStack = topExecuteStack();
			//Log.Warning("Probably here " + popBreakIndex + " / " + executeStack.Count() + " / " + topStack.verbSequence, true);
			bool oneOver = executeStack.Count() == popBreakIndex + 1;
			if(oneOver){
				this.popExecuteStack();
				return true;
			}

			if(!force){
				if(topStack.verbSequence.shouldRewindExecuteStack(this)){
					topStack.stackExecuted = false;
					return false;
				}
			}
			/**if(executeStack.Count() == popBreakIndex){
				return;
			}**/
			int indexNow = topStack.stackIndex;
			this.popExecuteStack();
			if(executeStack.Count() > 0){
				ExecuteStack tsNext = topExecuteStack();
				if(tsNext.verbSequence.scriptIndexMax(tsNext.subSequenceGroupIndex) > indexNow + 1){
					ExecuteStack nextGen = ExecuteStack.nextScriptExecutor();
					nextGen.verbSequence = tsNext.verbSequence.getSequenceAt(indexNow + 1, tsNext.subSequenceGroupIndex);
					nextGen.stackIndex = indexNow + 1;
					pushExecuteStack(nextGen);
				}else{
					tryPopWithChecks(popBreakIndex);
				}
			}else{
				if(verbScript.sequence.Count > indexNow + 1){
					ExecuteStack nextGen = ExecuteStack.nextScriptExecutor();
					nextGen.verbSequence = verbScript.sequence[indexNow + 1];
					nextGen.stackIndex = indexNow + 1;
					pushExecuteStack(nextGen);
				}else{
					//the end
				}
			}
			return true;
		}
		public void tryLoadFromSave(){
			for(int i = 0; i < this.loadStateExeStack.Count; i++){
				bool comp = (loadStateExeStack[i] & BF_Completed) != 0;
				int ide = loadStateExeStack[i] & 0x7FFFFFFF;
				int sss = loadSubSequenceGroupExeStack[i];
				ExecuteStack nextGen = ExecuteStack.nextScriptExecutor();
				nextGen.subSequenceGroupIndex = sss;
				if(i == 0){
					nextGen.verbSequence = verbScript.sequence[ide];
				}else{
					ExecuteStack topStack = topExecuteStack();
					nextGen.verbSequence = topStack.verbSequence.getSequenceAt(ide, topStack.subSequenceGroupIndex);
				}
				nextGen.stackIndex = ide;
				nextGen.stackExecuted = comp;
				pushExecuteStack(nextGen);
			}
			loadSubSequenceGroupExeStack.Clear();
			loadStateExeStack.Clear();
			for(int i = 0; i < scopePointerStack.Count; i++){
				scopeStack.Add(localVariableHolder.variables[(SCOPESLPREFIX + i).ToString()]);
			}
			notify_PostScopeStackChange();
			this.logTest();
		}
		public IEnumerable<object> func389813(int popBreakIndex){
			ExecuteStack tsNow = topExecuteStack();
			if(!tsNow.stackExecuted){
				IEnumerable<object> io = tsNow.tryExecute(this);
				foreach(object obj in io){
					yield return obj;
				}
				/**if(tsNow.verbSequence == null){
					Log.Warning("WARNING NULL VS " + tsNow.pv_verbSequenceLR.uniqueID);
				}**/
				if(tsNow.verbSequence.shouldSkipSubExecuteStack(this)){
					bool bOut = tryPopWithChecks(popBreakIndex, true);
				}else{
					tsNow.subSequenceGroupIndex = tsNow.verbSequence.scriptSequenceGroup(this);
					if(tsNow.verbSequence.scriptIndexMax(tsNow.subSequenceGroupIndex) > 0){
						ExecuteStack nextGen = ExecuteStack.nextScriptExecutor();
						nextGen.verbSequence = tsNow.verbSequence.getSequenceAt(0, tsNow.subSequenceGroupIndex);
						nextGen.stackIndex = 0;
						pushExecuteStack(nextGen);
					}
				}
			}else{
				tryPopWithChecks(popBreakIndex, false);
			}
			yield break;
		}
		
		public bool tryExecute(object __rootScope, int ticks = 1){
			rootScope = __rootScope;
            VerbSequence.ClearCache();
			if(executeStack.Count == 0 && !complete){
				if(loadStateExeStack.Count > 0){
					tryLoadFromSave();
				}else{
					if(verbScript.sequence.Count > 0){
						ExecuteStack nextGen = ExecuteStack.nextScriptExecutor();
						nextGen.verbSequence = verbScript.sequence[0];
						nextGen.stackIndex = 0;
						pushExecuteStack(nextGen);
					}
				}
			}
			int i = 0;
			while(ticks > 0 && executeStack.Count > 0){
				if(delayTicks > 0){
					if(delayTicks >= ticks){
						delayTicks -= ticks;
						ticks = 0;
						return false;
					}else{
						ticks -= delayTicks;
						delayTicks = 0;
					}
				}
				//Log.Warning("Suspect D " + Rand.Value + " / " + new System.Diagnostics.StackTrace(), true);
				func389813(-1).Count();
				i++;
				if(i > 100){
					throw new Exception("Possible infinite loop detected A " + executeStack.Count + " " + this.topExecuteStack().verbSequence.uniqueID);
				}
			}
			complete = true;
			if(delayTicks > 0){
				if(delayTicks >= ticks){
					delayTicks -= ticks;
					ticks = 0;
					return false;
				}else{
					ticks -= delayTicks;
					delayTicks = 0;
				}
			}
			//complete = true;
			//clearStack();
			//pushStack(root);
			//popStack();
			return true;
		}
		public IEnumerable<object> tryExecuteEnum0Delay(object __rootScope){
			rootScope = __rootScope;
            VerbSequence.ClearCache();
			if(executeStack.Count == 0 && !complete){
				if(loadStateExeStack.Count > 0){
					tryLoadFromSave();
				}else{
					if(verbScript.sequence.Count > 0){
						ExecuteStack nextGen = ExecuteStack.nextScriptExecutor();
						nextGen.verbSequence = verbScript.sequence[0];
						nextGen.stackIndex = 0;
						pushExecuteStack(nextGen);
					}
				}
			}
			int i = 0;
			while(executeStack.Count > 0){
				foreach(object obj in func389813(-1)){
					yield return obj;
				}
				i++;
				if(i > 100){
					throw new Exception("Possible infinite loop detected B " + executeStack.Count + " " + this.topExecuteStack().verbSequence.uniqueID);
				}
			}
			complete = true;
			yield break;
			//return true;
		}
	}
	public class ExecuteStack{
		public static List<ExecuteStack> free_scriptExecutor = new List<ExecuteStack>();
		public static ExecuteStack nextScriptExecutor(){
			if(free_scriptExecutor.Count > 0){
				return free_scriptExecutor.Pop();
			}
			return new ExecuteStack();
		}
		public void free(){
			free_scriptExecutor.Add(this);
			stackIndex = 0;
			subSequenceGroupIndex = 0;
			pv_verbSequence = null;
			stackExecuted = false;
		}
		
		public void logTest(){
			Log.Warning(stackIndex + " / " + stackExecuted + " / " + verbSequence.uniqueID);
		}

		public bool stackExecuted = false;
		public int stackIndex;
		public int subSequenceGroupIndex;
		private VerbSequence pv_verbSequence;
		
		public VerbSequence verbSequence{
			get{
				return pv_verbSequence;
			}
			set{
				pv_verbSequence = value;
			}
		}

		public IEnumerable<object> tryExecute(ExecuteStackContext executeContext){
			//Log.Warning("Suspect A " + Rand.Value + " / " + new System.Diagnostics.StackTrace());
			//Log.Warning("Execuyting " + stackIndex + " / " + verbSequence.uniqueID);
			/**foreach(object obj in verbSequence.evaluate(executeContext)){
			}**/
			//verbSequence.evaluate(executeContext)?.Count();
			//yield break;
			if(executeContext.ReturnType() == ReturnTypeStack.INTRAW && verbSequence is VerbCondition verC){
				IEnumerable<int> sa = verC.evaluateCT(executeContext);
				if(sa != null){
					foreach(int ob in sa){
						yield return ob;
					}
				}
			}else{
				IEnumerable<object> sa = verbSequence.evaluate(executeContext);
				if(sa != null){
					foreach(object ob in sa){
						yield return ob;
					}
				}
			}
			stackExecuted = true;
			//verbEffect.tryTick();//modify later
		}
	}
	
	public enum VariableType{
		Float,
		Vector3,
		Thing,
		Hediff,
		String,
		Unknown,
		UnknownValue,
		UnknownRef,
		UnknownDeep,
		UnknownDef
	}
	
	public enum VariableHolderType{
		TempScript,
		Local,
		Global //not used yet
	}
	public class VariableHolder : IExposable{
		public void logTest(){
			foreach(string str in variables.Keys){
				Log.Warning(str + " " + variableType[str] + " / " + variables[str]);
			}
		}

		public Dictionary<string, object> variables = new Dictionary<string, object>();
		public Dictionary<string, VariableType> variableType = new Dictionary<string, VariableType>();
		public void clear(){
			variables.Clear();
			variableType.Clear();
		}
		public void saveVariable(string key, object obj, VariableType var){
			if(variables.ContainsKey(key)){
				variables.Remove(key);
				variableType.Remove(key);
			}
			variables.Add(key, obj);
			variableType.Add(key, var);
		}
		public void removeVariable(string key){
			if(variables.ContainsKey(key)){
				variables.Remove(key);
				variableType.Remove(key);
			}
		}

		/**what ever
		public Dictionary<string, float> SAS_float = new Dictionary<string, float>();
		public Dictionary<string, string> SAS_string = new Dictionary<string, string>();
		public Dictionary<string, Vector3> SAS_Vector3 = new Dictionary<string, Vector3>();
		public Dictionary<string, Thing> SAS_Thing = new Dictionary<string, Thing>();
		public Dictionary<string, Hediff> SAS_Hediff = new Dictionary<string, Hediff>();
		**/

		public Dictionary<string, float> SAL_float;
		public Dictionary<string, string> SAL_string;
		public Dictionary<string, Vector3> SAL_Vector3;
		public Dictionary<string, Thing> SAL_Thing;
		public Dictionary<string, Hediff> SAL_Hediff;
		
		public List<string> SAL_ThingKey;
		public List<Thing> SAL_ThingValue;
		public List<string> SAL_HediffKey;
		public List<Hediff> SAL_HediffValue;
		
		public void ensureSAL(){
			if(SAL_float == null){ SAL_float = new Dictionary<string, float>(); }
			if(SAL_Thing == null){ SAL_Thing = new Dictionary<string, Thing>(); }
			if(SAL_Hediff == null){ SAL_Hediff = new Dictionary<string, Hediff>(); }
			if(SAL_Vector3 == null){ SAL_Vector3 = new Dictionary<string, Vector3>(); }
			if(SAL_string == null){ SAL_string = new Dictionary<string, string>(); }
		}
		public void freeSAL(){
			SAL_string = null;
			SAL_float = null;
			SAL_Vector3 = null;
			SAL_Thing = null;
			SAL_Hediff = null;
		}

		public void ExposeData(){
			switch(Scribe.mode){
				case LoadSaveMode.Inactive:
				case LoadSaveMode.PostLoadInit:
				case LoadSaveMode.ResolvingCrossRefs:
				case LoadSaveMode.LoadingVars:{
					ensureSAL();
					Scribe_Collections.Look<string, string>(ref SAL_string, "SAL_string", LookMode.Value, LookMode.Value);
					Scribe_Collections.Look<string, float>(ref SAL_float, "SAL_float", LookMode.Value, LookMode.Value);
					Scribe_Collections.Look<string, Vector3>(ref SAL_Vector3, "SAL_Vector3", LookMode.Value, LookMode.Value);
					Scribe_Collections.Look<string, Thing>(ref SAL_Thing, "SAL_Thing", LookMode.Value, LookMode.Reference, ref SAL_ThingKey, ref SAL_ThingValue);
					Scribe_Collections.Look<string, Hediff>(ref SAL_Hediff, "SAL_Hediff", LookMode.Value, LookMode.Reference, ref SAL_HediffKey, ref SAL_HediffValue);
					if(Scribe.mode == LoadSaveMode.PostLoadInit){
						if(SAL_string != null){
							foreach(KeyValuePair<string, string> kvpStringFloat in SAL_string){
								variables.Add(kvpStringFloat.Key, kvpStringFloat.Value);
								variableType.Add(kvpStringFloat.Key, VariableType.String);
							}
						}
						if(SAL_float != null){
							foreach(KeyValuePair<string, float> kvpStringFloat in SAL_float){
								variables.Add(kvpStringFloat.Key, kvpStringFloat.Value);
								variableType.Add(kvpStringFloat.Key, VariableType.Float);
							}
						}
						if(SAL_Vector3 != null){
							foreach(KeyValuePair<string, Vector3> kvpStringFloat in SAL_Vector3){
								variables.Add(kvpStringFloat.Key, kvpStringFloat.Value);
								variableType.Add(kvpStringFloat.Key, VariableType.Vector3);
							}
						}
						if(SAL_Thing != null){
							foreach(KeyValuePair<string, Thing> kvpStringFloat in SAL_Thing){
								variables.Add(kvpStringFloat.Key, kvpStringFloat.Value);
								variableType.Add(kvpStringFloat.Key, VariableType.Thing);
							}
						}
						if(SAL_Hediff != null){
							foreach(KeyValuePair<string, Hediff> kvpStringFloat in SAL_Hediff){
								variables.Add(kvpStringFloat.Key, kvpStringFloat.Value);
								variableType.Add(kvpStringFloat.Key, VariableType.Hediff);
							}
						}
						freeSAL();
					}
					break;
				}
				case LoadSaveMode.Saving:{
					ensureSAL();
					SAL_float.Clear();
					SAL_Thing.Clear();
					SAL_Hediff.Clear();
					SAL_Vector3.Clear();
					SAL_string.Clear();

					foreach(string key in variables.Keys){
						switch(variableType[key]){
							case VariableType.Float:{
								SAL_float.Add(key, (float)variables[key]);
								break;
							}
							case VariableType.Vector3:{
								SAL_Vector3.Add(key, (Vector3)variables[key]);
								break;
							}
							case VariableType.Thing:{
								SAL_Thing.Add(key, (Thing)variables[key]);
								break;
							}
							case VariableType.Hediff:{
								SAL_Hediff.Add(key, (Hediff)variables[key]);
								break;
							}
							case VariableType.String:{
								SAL_string.Add(key, (string)variables[key]);
								break;
							}
						}
					}
					Scribe_Collections.Look<string, float>(ref SAL_float, "SAL_float", LookMode.Value, LookMode.Value);
					Scribe_Collections.Look<string, Thing>(ref SAL_Thing, "SAL_Thing", LookMode.Value, LookMode.Reference);
					Scribe_Collections.Look<string, Hediff>(ref SAL_Hediff, "SAL_Hediff", LookMode.Value, LookMode.Reference);
					Scribe_Collections.Look<string, Vector3>(ref SAL_Vector3, "SAL_Vector3", LookMode.Value, LookMode.Value);
					Scribe_Collections.Look<string, string>(ref SAL_string, "SAL_string", LookMode.Value, LookMode.Value);
					break;
				}
			}

		}


	}
}
