//The inputs are the external factor which will be used to control the gamestate. 
using Godot;
using System;
using System.Collections.Generic;

public class InputControl : Node
{
    //Amount of input delay in frames.
    int input_delay = 5;
    //Max amount of frames to be able to rollback.
    int rollback = 7;

    //Input array to hold 256 inputs.
    Inputs[] input_array;
    //Queue for frame states of the past for rollback. 
    List<Frame_State[]> state_queue;

    //Ranges between 0-255 per circular input array cycle (cycle is every 256 frames)
     int frame_num = 0;

    //For testing state reset.
    bool canReset = true;

//----------Classes----------
    
    //Inputs for the local player during a single frame.
    public class Inputs
    {
        /*Indexing is as follows:[0] = W,[1] = A,[2] = S,[3] = D,[4] = Space */
        public bool[] local_input = new []{false, false, false, false, false};
    }

    public class Frame_State
    {
        //Contains the inputs by a local player for a single frame. 
        public bool[] local_input; 
        //State's frame number according to the 256 cycle.
        public int frame;
        //Contains the values needed for tracking the game's state at a given frame. 
        public Dictionary<string, int> game_state;

        public Frame_State(bool[] _local_input, int _frame, Dictionary<string, int> _game_state)
        {
            //An Array of booleans.
            this.local_input = _local_input;
            this.frame = _frame;
            /*Dicionary of the dictionaries.
            - game_state keys are child names, values are their individual state dictionaries.
            - state keys are state variables of the children (e.g. x and y), values are the variable values.
            */
            this.game_state = _game_state;
        }
    }

//----------Functions----------
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Initialize the input array.
        for(int i = 0; i < 256; ++i)
        {
            input_array[i] = new Inputs();
        }

        //Initialize the state queue.
        for(int i = 0; i < rollback; ++i)
        {
            state_queue[i] = new Frame_State(new bool [i],0,get_game_state());
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        handle_Input();
    }

    //Get input, rollback if necessary, call input execution
        public void handle_Input()
        {
            var pre_game_state = get_game_state();
            Inputs current_input = null;
            bool[] local_input = new bool[] {false, false, false, false, false}; 
    
            //For all children, set their update variables to their current/actual values.
            frame_start_all();

            //Record local inputs.
            if(Input.IsKeyPressed((int)KeyList.W))
            {
                local_input[0] = true;
            }
            if(Input.IsKeyPressed((int)KeyList.A))
            {
                local_input[1] = true;
            }
            if(Input.IsKeyPressed((int)KeyList.S))
            {
                local_input[2] = true;
            }
            if(Input.IsKeyPressed((int)KeyList.D))
            {
                local_input[3] = true;
            }
            if(Input.IsKeyPressed((int)KeyList.Space))
            {
                local_input[4] = true;
            }

            //Testing the state reset.
            if(Input.IsKeyPressed((int)KeyList.Enter) && (canReset && reset_stateAll(state_queue[0].gameState)))
            {
                canReset = false;
            }
            else 
            {
                canReset = true;
            }

            //What does this do #1
            input_array[(frame_num + input_delay) % 256].local_input = local_input;

            //What does this do #2
            current_input = new Inputs();
            //Unsure how to translate. 
            current_input.local_input = input_array[frame_num].local_input.Duplicate();

            //Update children with current input
            input_update_all(current_input);
            //Implement all applied updates/inputs to all child objects.
            execute_all();

            //Store current frame state into queue.
            state_queue.Add( new Frame_State(current_input.local_input, frame_num, pre_game_state));

            //Remove oldest state from queue.
            state_queue.RemoveAt(0);

            //Increment frame_num cycle.
            frame_num = (frame_num + 1) % 256;
        }
    
    public void frame_start_all()
    {
        foreach (Node child in GetChildren())
        {
            child.frame_start();
        }
    }

    public void reset_stateAll(Dictionary<string, int> gameState)
    {
        foreach (Node child in GetChildren())
        {
            child.reset_state(gameState);
        }
    }

    public void input_update_all(Inputs input)
    {
        foreach(Node child in GetChildren())
        {
            child.input_update();
        }
    }

    public void execute_all()
    {
        foreach(Node child in GetChildren())
        {
            child.execute();
        }
    }

    //Unsure how to translate.
    public string get_game_state()
    {
        foreach(Node child in GetChildren())
        {
            child.get_state();
        }

    }

}

