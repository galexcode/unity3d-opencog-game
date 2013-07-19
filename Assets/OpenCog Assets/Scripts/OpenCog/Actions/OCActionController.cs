
/// Unity3D OpenCog World Embodiment Program
/// Copyright (C) 2013  Novamente			
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Affero General Public License as
/// published by the Free Software Foundation, either version 3 of the
/// License, or (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU Affero General Public License for more details.
///
/// You should have received a copy of the GNU Affero General Public License
/// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections;
using System.Collections.Generic;
using Behave.Runtime;
using OpenCog.Actions;
using OpenCog.Attributes;
using OpenCog.Extensions;
using OpenCog.Map;
using ProtoBuf;
using UnityEngine;
using ContextType = BLOCBehaviours.ContextType;
using OCID = System.Guid;
using Tree = Behave.Runtime.Tree;
using TreeType = BLOCBehaviours.TreeType;
using System.Linq;
using System.Xml;
using OpenCog.Utility;
//using OpenCog.Aspects;

namespace OpenCog
{
	
namespace Actions
{

/// <summary>
/// The OpenCog OCRobotAgent.
/// </summary>
#region Class Attributes
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
[OCExposePropertyFields]
[Serializable]
#endregion
public class OCActionController : OCMonoBehaviour, IAgent
{

	//---------------------------------------------------------------------------

	#region Private Member Data

	//---------------------------------------------------------------------------
			
	[SerializeField]
	private TreeType _TreeType;
	private OCActionPlanStep _step = null;
	private Hashtable _idleParams;
	
	private static Dictionary<string, string> builtinActionMap = new Dictionary<string, string>();
			
				
	private Dictionary<string, TreeType> _ActionNameDictionary = new Dictionary<string, TreeType>()
	{ { "walk", TreeType.Character_Move }
	, { "grab", TreeType.Character_RightHandActivate }
	, { "eat", TreeType.Character_Destroy }
	, { "say", TreeType.Character_Tell }
	, { "jump_toward", TreeType.Character_Move }
	};
			
	// Assume that there's just one behaviour we'd like to execute at a given time
	private Dictionary<TreeType, Tree> _TreeTypeDictionary;
			
	// Our current queue of behaviours
	private Queue< OCActionPlanStep > _ActionPlanQueue;

	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Accessors and Mutators

	//---------------------------------------------------------------------------

	public TreeType TreeType 
	{
		get { return this._TreeType;}
		set {	_TreeType = value;}
	}

	public HashSet<string> RunningActions;

	public OCActionPlanStep Step 
	{
		get {return this._step;}
		set {_step = value;}
	}		
			
	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Public Member Functions

	//---------------------------------------------------------------------------

	public IEnumerator Start ()
	{
		_TreeTypeDictionary = new Dictionary<TreeType, Tree>();
		_ActionPlanQueue = new Queue<OCActionPlanStep>();
				
		foreach( TreeType type in Enum.GetValues( typeof(TreeType) ).Cast<TreeType>() )
		{
			_TreeTypeDictionary.Add(type, BLOCBehaviours.InstantiateTree( type, this ));
		}
				
		OCAction[] actions = gameObject.GetComponentsInChildren<OCAction>(true);
				
		foreach( Tree tree in _TreeTypeDictionary.Values )
		{
			if(tree == null)
				continue;
			int index = tree.Name.LastIndexOf(".")+1;
			if(index < 0 || index > tree.Name.Count())
				index = 0;
			string treeName = tree.Name.Substring(index);					
					
			foreach( OCAction action in actions)
			{
				if(action.FullName.Contains(treeName) || treeName.Contains("Behaviour"))
				{
					int actionTypeID = (int)Enum.Parse(typeof(BLOCBehaviours.ActionType), action.FullName);
				
					tree.SetTickForward( actionTypeID, action.ExecuteBehave );
				}
			}
		}
				
		OCActionPlanStep firstStep = new OCActionPlanStep();
		firstStep.Behaviour = _TreeTypeDictionary[_TreeType];
		firstStep.Arguments = new OCAction.OCActionArgs(gameObject, null, null);

		_ActionPlanQueue.Enqueue(firstStep);		

		RunningActions = new HashSet<string>();
		RunningActions.Add("StandIdleShow");

		while (Application.isPlaying) 
		{
			yield return new WaitForSeconds (1.0f / 120.0f);
			UpdateAI ();
		}
	}
	
//			private void TestProprioception()
//			{
//				Debug.Log ("RobotAgent's Transform is at position [" + this.transform.position.x + ", " + this.transform.position.y + ", " + this.transform.position.z + "]");
//		
//				Map map = (Map)GameObject.FindObjectOfType (typeof(Map));
//				
//				float characterHeight = this.GetComponent<CharacterController>().height;
//				
//				//map.IsPathOpen(this.transform, characterHeight, Map.PathDirection.ForwardWalk);
//				//map.IsPathOpen(this.transform, characterHeight, Map.PathDirection.ForwardRun);
//				//map.IsPathOpen(this.transform, characterHeight, Map.PathDirection.ForwardRun);
//				//map.IsPathOpen(this.transform, characterHeight, Map.PathDirection.ForwardJump);
//				//map.IsPathOpen(this.transform, characterHeight, Map.PathDirection.ForwardClimb);
//				//map.IsPathOpen(this.transform, characterHeight, Map.PathDirection.ForwardDrop);
//			}

	public void Update ()
	{
//		if(Time.frameCount%120 == 0)
//		{
//			var bestWeight = -1.0;
//			String playing = "";
//			foreach (AnimationState s in animation)
//			{
//    		if (s.enabled && s.weight > bestWeight)
//				{
//       		playing += s.name + " ";
//        	bestWeight = s.weight;
//    		}
//			}
//			Debug.Log("Animation State: " + playing);
//		}
		
//				this.TestProprioception ();
	}

	public BehaveResult	 Tick (Tree sender, bool init)
	{
//			Debug.Log
//			(
//				"Got ticked by unhandled " + (BLOpenCogCharacterBehaviours.IsAction( sender.ActiveID ) ? "action" : "decorator")
//			+ ( BLOpenCogCharacterBehaviours.IsAction( sender.ActiveID )
//				? ((BLOpenCogCharacterBehaviours.ActionType)sender.ActiveID).ToString()
//				: ((BLOpenCogCharacterBehaviours.DecoratorType)sender.ActiveID).ToString()
//				)
//			);

		return BehaveResult.Failure;
	}

//	public BehaveResult TickIdleAction(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
//	{
//			Debug.Log("In Robot Idle...");
//
//			return BehaveResult.Success;
//	}

//	public BehaveResult FallAction 
//	{
//		get {
//			OCFallForwardAction action = gameObject.GetComponent<OCFallForwardAction> ();
//
//			BehaveResult ret = DefaultActionTickHandler (action);
//
//			OpenCog.Map.OCMap map = (OpenCog.Map.OCMap)GameObject.FindObjectOfType (typeof(OpenCog.Map.OCMap));
//
//			if (ret != BehaveResult.Success)
//				return ret;
//
//			CharacterController charController = gameObject.GetComponent<CharacterController> ();
//
//			Vector3 robotPos = gameObject.transform.position;
//			Vector3 distanceVec = ((Vector3)TargetBlockPos) - robotPos;
//			float robotForwardDistance = Vector3.Dot (distanceVec, gameObject.transform.forward);
//			float robotUpDistance = Vector3.Dot (distanceVec, gameObject.transform.up);
//			float robotRightDistance = Vector3.Dot (distanceVec, gameObject.transform.right);
//			float robotLeftDistance = Vector3.Dot (distanceVec, -gameObject.transform.right);
//
//			if (TargetBlockPos != Vector3i.zero
//				&& map.IsPathOpen(transform, charController.height, OpenCog.Map.OCMap.PathDirection.ForwardDrop)
////					&& robotForwardDistance <= 2.5f
//			&& robotForwardDistance >= 0.5f
////					&& robotRightDistance < 0.5f
////					&& robotLeftDistance < 0.5f
////					&& charController.isGrounded
//			) {
//				action.Execute ();
//				//Debug.Log("In OCRobotAgent.FallAction, " + action.GetType() + " Success");
//				return BehaveResult.Success;
//			}
//
//			//Debug.Log("In OCRobotAgent.FallAction, " + action.GetType() + " Failure");
//			return BehaveResult.Failure;
//		}
//
//		set {
//		}
//	}
//
//	public BehaveResult IdleAction 
//	{
//	// tick handler
//		get {
//			OCIdleAction action = gameObject.GetComponent<OCIdleAction> ();
//			CharacterController charController = gameObject.GetComponent<CharacterController> ();
//
//			BehaveResult ret = DefaultActionTickHandler (action);
//
//			if (ret != BehaveResult.Success)
//				return ret;
//
//			if (TargetBlockPos == Vector3i.zero && charController.isGrounded) {
//				action.Execute ();
//				//Debug.Log("In OCRobotAgent.IdleAction, " + action.GetType() + " Success");
//				return BehaveResult.Success;
//			}
//
//			//Debug.Log("In OCRobotAgent.IdleAction, " + action.GetType() + " Failure");
//			return BehaveResult.Failure;
//		}
//
//	// reset handler
//		set {
//		}
//	}
//
//	public BehaveResult ClimbAction 
//	{
//	// tick handler
//		get {
//			OCClimbUpAction action = gameObject.GetComponent<OCClimbUpAction> ();
//
//			BehaveResult ret = DefaultActionTickHandler (action);
//
//			OpenCog.Map.OCMap map = (OpenCog.Map.OCMap)GameObject.FindObjectOfType (typeof(OpenCog.Map.OCMap));
//
//			if (ret != BehaveResult.Success)
//				return ret;
//
//			CharacterController charController = gameObject.GetComponent<CharacterController> ();
//
//			Vector3 robotPos = gameObject.transform.position;
//			Vector3 distanceVec = ((Vector3)TargetBlockPos) - robotPos;
//			float robotUpDistance = Vector3.Dot (distanceVec, gameObject.transform.up);
//			float robotForwardDistance = Vector3.Dot (distanceVec, gameObject.transform.forward);
//
//			if (TargetBlockPos != Vector3i.zero
//			//&& robotUpDistance >= 1.5f
//			&& robotForwardDistance >= 0.5f
//			&& map.IsPathOpen(transform, charController.height, OpenCog.Map.OCMap.PathDirection.ForwardClimb)
//			&& charController.isGrounded) {
//				action.Execute ();
//				//Debug.Log("In OCRobotAgent.ClimbAction, " + action.GetType() + " Success");
//				return BehaveResult.Success;
//			}
//
//			//Debug.Log("In OCRobotAgent.ClimbAction, " + action.GetType() + " Failure");
//			return BehaveResult.Failure;
//		}
//
//	// reset handler
//		set {
//		}
//	}
//
//	public BehaveResult RunAction 
//	{
//	// tick handler
//		get {
//			OCRunForwardAction action = gameObject.GetComponent<OCRunForwardAction> ();
//
//			BehaveResult ret = DefaultActionTickHandler (action);
//
//			OpenCog.Map.OCMap map = (OpenCog.Map.OCMap)GameObject.FindObjectOfType (typeof(OpenCog.Map.OCMap));
//
//			if (ret != BehaveResult.Success)
//				return ret;
//
//			CharacterController charController = gameObject.GetComponent<CharacterController> ();
//
//			Vector3 robotPos = gameObject.transform.position;
//			Vector3 distanceVec = ((Vector3)TargetBlockPos) - robotPos;
//			float robotForwardDistance = Vector3.Dot (distanceVec, gameObject.transform.forward);
//			float robotUpDistance = Vector3.Dot (distanceVec, gameObject.transform.up);
//			float robotRightDistance = Vector3.Dot (distanceVec, gameObject.transform.right);
//			float robotLeftDistance = Vector3.Dot (distanceVec, -gameObject.transform.right);
//
//			if (TargetBlockPos != Vector3i.zero
//				&& map.IsPathOpen(transform, charController.height, OpenCog.Map.OCMap.PathDirection.ForwardRun)
//			&& robotForwardDistance > 3.5f
////					&& robotRightDistance < 1.5f
////					&& robotLeftDistance < 1.5f
//			&& charController.isGrounded
//			) {
//				action.Execute ();
//				//Debug.Log ("In OCRobotAgent.RunAction, " + action.GetType () + " Success");
//				return BehaveResult.Success;
//			}
//
//			//Debug.Log ("In OCRobotAgent.RunAction, " + action.GetType () + " Failure");
//			return BehaveResult.Failure;
//		}
//
//	// reset handler
//		set {
//		}
//	}
//
//	public BehaveResult JumpAction 
//	{
//	// tick handler
//		get {
//			OCJumpForwardAction action = gameObject.GetComponent<OCJumpForwardAction> ();
//
//			BehaveResult ret = DefaultActionTickHandler (action);
//
//			OpenCog.Map.OCMap map = (OpenCog.Map.OCMap)GameObject.FindObjectOfType (typeof(OpenCog.Map.OCMap));
//
//			if (ret != BehaveResult.Success)
//				return ret;
//
//			CharacterController charController = gameObject.GetComponent<CharacterController> ();
//
//			Vector3 robotPos = gameObject.transform.position;
//			Vector3 distanceVec = ((Vector3)TargetBlockPos) - robotPos;
//			float robotUpDistance = Vector3.Dot (distanceVec, gameObject.transform.up);
//			float robotForwardDistance = Vector3.Dot (distanceVec, gameObject.transform.forward);
//
//			if (TargetBlockPos != Vector3i.zero
//				&& map.IsPathOpen(transform, charController.height, OpenCog.Map.OCMap.PathDirection.ForwardJump)
//			&& robotUpDistance >= 1.5f
//			&& robotForwardDistance >= 0.0f
//			&& charController.isGrounded) {
//				action.Execute ();
//				//Debug.Log("In OCRobotAgent.JumpAction, " + action.GetType() + " Success");
//				return BehaveResult.Success;
//			}
//
//			//Debug.Log("In OCRobotAgent.JumpAction, " + action.GetType() + " Failure");
//			return BehaveResult.Failure;
//		}
//
//	// reset handler
//		set {
//		}
//	}
//
//	public BehaveResult TurnLeftAction 
//	{
//	// tick handler
//		get {
//			OCTurnLeftAction action = gameObject.GetComponent<OCTurnLeftAction> ();
//
//			BehaveResult ret = DefaultActionTickHandler (action);
//
//			if (ret != BehaveResult.Success)
//				return ret;
//
//			CharacterController charController = gameObject.GetComponent<CharacterController> ();
//
//			Vector3 robotPos = gameObject.transform.position;
//			Vector3 distanceVec = ((Vector3)TargetBlockPos) - robotPos;
//			float robotForwardDistance = Vector3.Dot (distanceVec, gameObject.transform.forward);
//			float robotRightDistance = Vector3.Dot (distanceVec, gameObject.transform.right);
//			float robotLeftDistance = Vector3.Dot (distanceVec, -gameObject.transform.right);
//
//			if (TargetBlockPos != Vector3i.zero
//				&& ( robotLeftDistance >= 0.5f || robotForwardDistance < 0.0f)
//				&& charController.isGrounded) {
//				action.Execute ();
//				//Debug.Log("In OCRobotAgent.TurnLeftAction, " + action.GetType() + " Success");
//				return BehaveResult.Success;
//			}
//
//			//Debug.Log("In OCRobotAgent.TurnLeftAction, " + action.GetType() + " Failure");
//			return BehaveResult.Failure;
//		}
//
//	// reset handler
//		set {
//		}
//	}
//
//	public BehaveResult TurnRightAction 
//	{
//	// tick handler
//		get {
//			OCTurnRightAction action = gameObject.GetComponent<OCTurnRightAction> ();
//
//			BehaveResult ret = DefaultActionTickHandler (action);
//
//			if (ret != BehaveResult.Success)
//				return ret;
//
//			CharacterController charController = gameObject.GetComponent<CharacterController> ();
//
//			Vector3 robotPos = gameObject.transform.position;
//			Vector3 distanceVec = ((Vector3)TargetBlockPos) - robotPos;
//			float robotForwardDistance = Vector3.Dot (distanceVec, gameObject.transform.forward);
//			float robotRightDistance = Vector3.Dot (distanceVec, gameObject.transform.right);
//			float robotLeftDistance = Vector3.Dot (distanceVec, -gameObject.transform.right);
//
//			if (TargetBlockPos != Vector3i.zero
//				&& robotRightDistance >= 0.5f
//				&& charController.isGrounded) {
//				action.Execute ();
//				//Debug.Log("In OCRobotAgent.TurnRightAction, " + action.GetType() + " Success");
//				return BehaveResult.Success;
//			}
//
//			//Debug.Log("In OCRobotAgent.TurnRightAction, " + action.GetType() + " Failure");
//			return BehaveResult.Failure;
//		}
//
//	// reset handler
//		set {
//		}
//	}
//
//	public BehaveResult WalkAction 
//	{
//	// tick handler
//		get {
//			OCWalkForwardAction action = gameObject.GetComponent<OCWalkForwardAction> ();
//
//			BehaveResult ret = DefaultActionTickHandler (action);
//
//			OpenCog.Map.OCMap map = (OpenCog.Map.OCMap)GameObject.FindObjectOfType (typeof(OpenCog.Map.OCMap));
//
//			if (ret != BehaveResult.Success)
//				return ret;
//
//			CharacterController charController = gameObject.GetComponent<CharacterController> ();
//
//			Vector3 robotPos = gameObject.transform.position;
//			Vector3 distanceVec = ((Vector3)TargetBlockPos) - robotPos;
//			float robotForwardDistance = Vector3.Dot (distanceVec, gameObject.transform.forward);
//			float robotUpDistance = Vector3.Dot (distanceVec, gameObject.transform.up);
//			float robotRightDistance = Vector3.Dot (distanceVec, gameObject.transform.right);
//			float robotLeftDistance = Vector3.Dot (distanceVec, -gameObject.transform.right);
//
//			if (TargetBlockPos != Vector3i.zero
//				&& map.IsPathOpen(transform, charController.height, OpenCog.Map.OCMap.PathDirection.ForwardWalk)
//			&& robotForwardDistance <= 3.5f
//			&& robotForwardDistance >= 0.5f
//			&& robotUpDistance <= 1.5f
////					&& robotRightDistance < 0.5f
////					&& robotLeftDistance < 0.5f
//			&& charController.isGrounded
//			) {
//				action.Execute ();
//				//Debug.Log("In OCRobotAgent.WalkAction, " + action.GetType() + " Success");
//				return BehaveResult.Success;
//			}
//
//			//Debug.Log("In OCRobotAgent.WalkAction, " + action.GetType() + " Failure");
//			return BehaveResult.Failure;
//		}
//
//	// reset handler
//		set {
//		}
//	}
			
	// Map XML elements to high-, mid-, or low-behaviour trees.
	// Then queue each element in the plan in the behaviour queue.
	public void ReceiveActionPlan(List<XmlElement> actionPlan)
	{
//		Debug.Log("In ReceiveActionPlan...");
//				
//		string actionName = GetAttribute(element, OCEmbodimentXMLTags.NAME_ATTRIBUTE);
//
//	  int sequence = int.Parse(GetAttribute(element, OCEmbodimentXMLTags.SEQUENCE_ATTRIBUTE));
//	  ArrayList paramList = new ArrayList();
//	
//	  XmlNodeList list = GetChildren(element, OCEmbodimentXMLTags.PARAMETER_ELEMENT);
//	  // Extract parameters from the xml element.
//	  for (int i = 0; i < list.Count; i++)
//	  {
//      XmlElement parameterElement = (XmlElement)list.Item(i);
//      ActionParamType parameterType = ActionParamType.getFromName(parameterElement.GetAttribute(OCEmbodimentXMLTags.TYPE_ATTRIBUTE));
//
//      switch (parameterType.getCode())
//      {
//        case ActionParamTypeCode.VECTOR_CODE:
//          XmlElement vectorElement = ((XmlElement)(GetChildren(parameterElement, OCEmbodimentXMLTags.VECTOR_ELEMENT)).Item(0));
//          float x = float.Parse(GetAttribute(vectorElement, OCEmbodimentXMLTags.X_ATTRIBUTE), CultureInfo.InvariantCulture.NumberFormat);
//          float y = float.Parse(GetAttribute(vectorElement, OCEmbodimentXMLTags.Y_ATTRIBUTE), CultureInfo.InvariantCulture.NumberFormat);
//          float z = float.Parse(GetAttribute(vectorElement, OCEmbodimentXMLTags.Z_ATTRIBUTE), CultureInfo.InvariantCulture.NumberFormat);
//
//					if (adjustCoordinate)
//					{
//						x += 0.5f;
//						y += 0.5f;
//						z += 0.5f;
//					}
//
//          // swap z and y
//          paramList.Add(new Vector3(x, z, y)); 
//          break;
//        case ActionParamTypeCode.BOOLEAN_CODE:
//          paramList.Add(Boolean.Parse(GetAttribute(parameterElement, OCEmbodimentXMLTags.VALUE_ATTRIBUTE)));
//          break;
//        case ActionParamTypeCode.INT_CODE:
//          paramList.Add(int.Parse(GetAttribute(parameterElement, OCEmbodimentXMLTags.VALUE_ATTRIBUTE)));
//          break;
//        case ActionParamTypeCode.FLOAT_CODE:
//          paramList.Add(float.Parse(GetAttribute(parameterElement, OCEmbodimentXMLTags.VALUE_ATTRIBUTE)));
//          break;
//        case ActionParamTypeCode.ROTATION_CODE:
//          //!! This is a hacky trick. For currently, we do not use rotation
//          // in rotate method, so just convert it to vector type. What's more,
//          // "RotateTo" needs an angle parameter.
//
//          // Trick... add an angle...
//          XmlElement rotationElement = ((XmlElement)(GetChildren(parameterElement, OCEmbodimentXMLTags.ROTATION_ELEMENT)).Item(0));
//          float pitch = float.Parse(GetAttribute(rotationElement, OCEmbodimentXMLTags.PITCH_ATTRIBUTE), CultureInfo.InvariantCulture.NumberFormat);
//          float roll = float.Parse(GetAttribute(rotationElement, OCEmbodimentXMLTags.ROLL_ATTRIBUTE), CultureInfo.InvariantCulture.NumberFormat);
//          float yaw = float.Parse(GetAttribute(rotationElement, OCEmbodimentXMLTags.YAW_ATTRIBUTE), CultureInfo.InvariantCulture.NumberFormat);
//
//          Rotation rot = new Rotation(pitch, roll, yaw);
//          Vector3 rot3 = new Vector3(rot.Pitch, rot.Roll, rot.Yaw);
//
//          paramList.Add(0.0f);
//          paramList.Add(rot3);
//          break;
//        case ActionParamTypeCode.ENTITY_CODE:
//          // This action is supposed to act on certain entity.
//          XmlElement entityElement = ((XmlElement)(GetChildren(parameterElement, OCEmbodimentXMLTags.ENTITY_ELEMENT)).Item(0));
//
//          int id = int.Parse(GetAttribute(entityElement, OCEmbodimentXMLTags.ID_ATTRIBUTE));
//          string type = GetAttribute(entityElement, OCEmbodimentXMLTags.TYPE_ATTRIBUTE);
//          ActionTarget target = new ActionTarget(id, type);
//
//          paramList.Add(target);
//          break;
//        default:
//          paramList.Add(GetAttribute(parameterElement, OCEmbodimentXMLTags.VALUE_ATTRIBUTE));
//          break;
//      }
//	  } 
	}
			
	public void LoadActionPlanStep(string actionName, OCAction.OCActionArgs arguments)
	{
		TreeType treeType = _ActionNameDictionary[actionName];
		Tree tree = _TreeTypeDictionary[treeType];
		OCActionPlanStep actionPlanStep = new OCActionPlanStep();
		actionPlanStep.Behaviour = tree;
		actionPlanStep.Arguments = arguments;
		_ActionPlanQueue.Enqueue(actionPlanStep);
	}
			
	public void CancelActionPlan()
	{
		_step.Behaviour.Reset();
		_ActionPlanQueue.Clear();
	}
	
	public void UpdateAI ()
	{
 		if(_step == null && _ActionPlanQueue.Count != 0)
		{
			_step = _ActionPlanQueue.Dequeue();
		}
		else if(_step == null && _ActionPlanQueue.Count == 0)
		{
			OCActionPlanStep step = new OCActionPlanStep();
			step.Behaviour = _TreeTypeDictionary[_TreeType];
			step.Arguments = new OCAction.OCActionArgs(gameObject, null, null);
			_step = step;
		}
				
		BehaveResult result = _step.Behaviour.Tick ();
				
		if(result != BehaveResult.Running)
		{
			if(_step.Arguments.ActionPlanID != null && _ActionPlanQueue.Count == 1)
				OCConnectorSingleton.Instance.SendActionPlanStatus(_step.Arguments.ActionPlanID, result == BehaveResult.Success);						
					
			_step.Behaviour.Reset();
			if(_ActionPlanQueue.Count == 0) _ActionPlanQueue.Enqueue(_step);	
			_step = _ActionPlanQueue.Dequeue();
			//if(result == BehaveResult.Success) Debug.Log("In OCActionController.UpdateAI, Result: " + result.ToString());
		}
		
	}

	public void	 Reset (Tree sender)
	{
	}

	public int	 SelectTopPriority (Tree sender, params int[] IDs)
	{
		return IDs [0];
	}

	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Private Member Functions

	//---------------------------------------------------------------------------

//	private BehaveResult DefaultActionTickHandler (OCAction action)
//	{
//		Vector3 robotPos = gameObject.transform.position;
//		Vector3 distanceVec = ((Vector3)TargetBlockPos) - robotPos;
//		
//		if (action == null)
//			return BehaveResult.Failure;
//
//		if (action.ShouldTerminate ()) {
//			//action.Terminate();
//			//Debug.Log ("In OCRobotAgent.DefaultActionTickHandler, " + action.GetType () + " Failure");
//			return BehaveResult.Failure;
//		}
//
//		if (action.IsExecuting ()) {
//			//Debug.Log("In OCRobotAgent.DefaultActionTickHandler, " + action.GetType() + " Running");
//			return BehaveResult.Running;
//		}
//
//		if (TargetBlockPos != Vector3i.zero) {
//			//Debug.Log("In OCRobotAgent.DefaultActionTickHandler, Distance to TNT block is: " + distanceVec.magnitude + ", Vector is: " + distanceVec);
//		}
//
//		return BehaveResult.Success;
//	}
	
	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Other Members

	//---------------------------------------------------------------------------

	public class OCActionPlanStep
	{
		private Behave.Runtime.Tree _behaviour;
		private OCAction.OCActionArgs _arguments;
		
		public OCAction.OCActionArgs Arguments 
		{
			get 
			{
				return this._arguments;
			}
			set 
			{
				_arguments = value;
			}
		}

		public Tree Behaviour 
		{
			get 
			{
				return this._behaviour;
			}
			set 
			{
				_behaviour = value;
			}
		}
	}
			
	// TODO: This cose is just a set of stubs to get rid of an error.
	//public static event ActionCompleteHandler globalActionCompleteEvent;
	//public delegate void ActionCompleteHandler(OCAction action);
	// Removed...due to the fact that OCConnector will be polling this class for action status updates.

	// TODO: Implement / remove build block function which can be called by OCConnector.ParseSingleActionElement. Will probably have to be amended with material / blockdata.
	public void BuildBlockAtPosition(Vector3i desiredBlockLocation)
	{

	}

	// TODO: Implement / remove move to location function which can be called by OCConnector.ParseSingleActionElement.
	public void MoveToCoordinate(Vector3 desiredLocation)
	{

	}

	// TODO: Implement function below properly.
	public static string GetOCActionNameFromMap(string methodName)
	{
		if (builtinActionMap.ContainsValue(methodName))
		{
			foreach (KeyValuePair<string, string> pair in builtinActionMap)
			{
				if (pair.Value == methodName) return pair.Key;
			}
		}

		return null;
	}
			
	// TODO: Implement dynamic behaviour tree loading to execute actions
	public void StartAction(OCAction action, OCID sourceID, OCID targetStartID, OCID targetEndID)
	{
	}
			
	public Dictionary<string, OCAction> GetCurrentActions()
	{
		return new Dictionary<string, OCAction>();
	}

	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

}// class OCRobotAgent

}// namespace Character

}// namespace OpenCog




