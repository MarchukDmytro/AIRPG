import os
import sys
import time
import json
import random
import ollama
import argparse
import subprocess
from os import path
from rapidfuzz import fuzz
from collections import deque
from datetime import datetime, UTC


sys.stdout.reconfigure(encoding='utf-8')

RUN_ID = f"run_{int(time.time())}"
BACKLOG_FILE = f"Worlds/logs/llm_backlog_{RUN_ID}_{datetime.now(UTC).strftime('%Y-%m-%d_%H-%M-%S')}.jsonl"

def log_backlog(
    phase,
    persona,
    task,
    context,
    options,
    response,
    iteration=None,
    critic_id=None,
    model=None
):
    entry = {
        "timestamp": datetime.now(UTC).isoformat(),
        "run_id": RUN_ID,
        "iteration": iteration,
        "phase": phase,
        "critic_id": critic_id,
        "model": model,
        "persona_prompt": persona,
        "task_prompt": task,
        "context": context,
        "options": options,
        "response": response
    }

    with open(BACKLOG_FILE, "a", encoding="utf-8") as f:
        f.write(json.dumps(entry, ensure_ascii=False) + "\n")
    
#personas
def get_builder_prompt():
    builder_prompt = "You are a Master D&D Worldbuilder/Writer. You specialize in gritty, logical, and immersive fantasy. Your writing is evocative and organized. You work with Editor that will give you castomer guidlines that you shoud base you world on. After your world is critiqued by the Council of Critics, you will receive a Master Action Plan that you must follow to fix the logical errors and deepen the lore.Your output will be presented to a customer so format in a way that describe only world and dont mention inner workings of production like action plan or critics."

    return builder_prompt

def get_critic_prompt():    
    critic_prompt = "You are a Harsh but Honest Experienced Worldbuilding and Lore Critic. You have an eye for logical consistency, internal realism, and realistic depth. Your critiques are ruthless but constructive, designed to expose flaws and elevate the worldbuilding to a professional standard."
    return critic_prompt

def get_synthesizer_prompt():
    syntesizer_prompt = "You are the Chief Editor, team leader that can manage multiple critiques and synthesize them into a cohesive plan."
    return syntesizer_prompt

#tasks
def get_task(task,):
    """
    task can be one of: "builder", "critic", "synthesizer", "fix"
    """
    
    
    task_prompts = {
        "builder": """
Take the World Seed provided and extrapolate it into a fully realized, living reality. Your goal is not to list facts, but to weave a deep, interconnected tapestry of lore that feels ancient and lived-in.

Use the Seed as the DNA of this world. Let the consequences of the magic system, the genre, and the geography ripple out to touch every aspect of existence. Explore how these fundamental forces shape the culture, the economy, the architecture, and the inevitable conflicts between factions. Do not stop at the surface level—dig into the history, the secrets, and the contradictions that make a world feel real.

As you write, naturally converge on a specific starting region. Flesh this area out in high resolution—its sights, sounds, local politics, and looming threats—serving as the perfect entry point for an adventure.

Be exhaustive. Follow interesting threads wherever they lead. Prioritize originality, internal logic, and immersive detail over brevity. Write until the world feels complete.
""",
#~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        "seed": """
You are the Lead World Architect. Your goal is NOT to write lore yet, but to design the 'Meta-Architecture' and 'Structural Skeleton' that the lore will be built upon.

Construct the DNA of this setting by defining the following Meta-Layers:

1. THE GENRE SYNTHESIS & TONAL PALETTE: 
   Don't just pick a genre. Fuse several distinct concepts (e.g., 'Industrial Espionage' meets 'High Druidic Fantasy') to create a unique narrative physics. Define the 'Mood' that permeates the table—is it hopeful, decaying, paranoid, or whimsical?

2. THE SYSTEMIC ENGINE (Magic & Technology):
   Define the 'Rules of Reality.' How does the supernatural or technological define the hierarchy of power? Do not just describe the magic; describe its *consequences* on economy, war, and daily survival. How does this system fundamentally alter the trajectory of civilization compared to the real world?

3. THE CENTRAL TENSION (The World's heartbeat):
   What is the singular, overarching conflict or 'Great Truth' that drives this setting? Is it Man vs. Nature? Chaos vs. Order? The Past vs. The Future?

4. THE STRUCTURAL SKELETON (15 Design Pillars):
   Formulate 15 deep, provocative 'World-Building Questions' that must be answered to flesh out this skeleton. 
   - These questions should target: Geography, Geopolitics, Economy, Religion, and Hidden Mysteries.
   - Do not ask yes/no questions. Ask complex 'How' and 'Why' questions that require massive creative answers.
   - These 15 questions will serve as the prompt for the next phase of creation.

OUTPUT GOAL: A high-level design document that serves as the 'Source Code' for a massive campaign setting.
""",
#~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        "critic": """
Analyze the lore provided with the dual mindset of a Lead Continuity Editor and a Creative Director.

Your primary mandate is to ruthlessly hunt down logical inconsistencies and causal breaks. Stress-test the world's internal rules:
- If a rule is established, is it followed everywhere?
- Do the consequences of history and geography make sense?
- Are there contradictions in how factions or mechanics operate?

- How can world be expanded and deepend, enreached? Create logical subquestions that the lore should answer to feel more complete? Dont let writer to stuck on big general World-Building Question and make him deep think through smaller details taht make world alive.

Simultaneously, challenge the creative depth. Look for shallow justifications or convenient plot armor that weaken the setting's believability.

Shine a light on the cracks in the logic and the hollow spots in the worldbuilding. Force the writer to confront these errors and bridge the gaps themselves.

Analyze the lore for logic gaps, inconsistencies, and cliches.Be ruthless in your analysis. End you critique with a conclusion and final score out of 100 like 80/100.
""",
#~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        "synthesizer": """
Read throu all the critiques provided by multiple critics and analyze thair satisfaction and fina scores, if majority of critics are satisfied or have 80+ score out of 100, conclude that the world is ready and answer with  [SATISFIED] in your response, otherwise if majority of critics are not satisfied or see room for improvments act according to next directives:
Your task is to synthesize the five provided critiques into a single, cohesive Master Action Plan for the Worldbuilder. 
Read through the feedback to identify structural flaws, logical inconsistencies, and thematic contradictions. 
If the critics disagree with each other, use majority rule or your judgment if critiques are even to prioritize the feedback that best strengthens the internal logic and realism of the setting.
Output your response as a clear, authoritative set of instructions that tells the Writer exactly what needs to be fixed to elevate the lore to a professional standard.
Compile questions and suggestions from all critiques into a single, prioritized list of actionable directives. Each directive should be specific and focused on addressing a particular flaw or weakness in the lore. The directives should be clear enough that the Writer can understand exactly what needs to be changed, added, or removed to fix the identified issues.
Compile questions and suggestions from all critiques into a single list of questions that the Writer shoud answer and explore to deepen the lore and make it more immersive. These questions should encourage the Writer to think through the implications of the worldbuilding and add rich detail that makes the setting feel alive and lived-in.
\n\n
Rememebr, if majority of critics are satisfied or have 80+ score out of 100, conclude that the world is ready and answer with  [SATISFIED] in your response.
""",
#~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        "fix": """Fix and expand the world lore based on this Action Plan. Keep the strong parts, but solve all logical errors.
Your mandate is two-fold:
1. INTEGRATE & POLISH Address every directive in the Action Plan. Solve all identified logical errors, contradictions, and weak justifications. The fix must be seamless—weave the solution into the existing lore so the world's structure is logically bulletproof.
2. EXPAND & ENRICH: While fixing the flaws, use the opportunity to deepen the lore. Keep all the original, strong, and unique elements. Add detail, history, and evocative descriptions to the revised sections to elevate the writing quality.
"""
    }
    
    
    return task_prompts.get(task, "")  

def get_critic_options(critic_config):
    opts = critic_config.copy()
    opts["seed"] = random.randint(0, 2**31)
    return opts

def ask_ai(
    persona,
    task,
    context="",
    options=None,
    phase="UNSPECIFIED",
    iteration=None,
    critic_id=None,
    args=None
):
    model = args.model
    streaming = args.stream
    start_time = time.time()
    max_repeat_sentense = 25
    
    messages = [
        {'role': 'system', 'content': persona},
        {'role': 'user', 'content': f"CONTEXT:\n{context}\n\nTASK:\n{task}"}
    ]
    stream = ollama.chat(
        model=model,
        messages=messages,
        options=options,
        stream=True
    )
    
    full_content = []
    last_chunks = []
    sentence = []
    
    last_sentences = deque(maxlen=max_repeat_sentense)
    
    for chunk in stream:
        
        token = chunk["message"]["content"]
        
        full_content.append(token)
        last_chunks.append(token)
        sentence.append(token)
        
        #fitering for repetition and logical loops
        if any(p in token for p in ".!?"):  # end of sentence and sentence is reasonably long
            if len(sentence) < 20:
                sentence.clear()
                continue
            current = "".join(sentence).strip()
            sentence.clear()

            last_sentences.append(current)
            abort = False
            # Only check when window is full
            if len(last_sentences) == max_repeat_sentense:
                for i in range(max_repeat_sentense):
                    
                    if abort:break
                    
                    for j in range(i+1, max_repeat_sentense):
                        score = fuzz.ratio(last_sentences[i], last_sentences[j])
                        if score >= 90:  # high similarity threshold
                            print(
                                f"\n[!] Repetition detected — aborting stream \n\n"
                                f"Repeated sentence: {current}\n\n"
                            )
                            abort = True
                            break
            if abort:break
            last_sentences.clear()
                
                            
                            
                
        if len(last_chunks) > 10:
            if len(set(last_chunks)) == 1:  # all tokens in the window are the same
                print(f"\n[!] Repetition detected — aborting stream {last_chunks}\n{full_content}\n")
                break
            last_chunks.pop(0)
        #stream print in terminal
        if streaming:
            print(token, end="", flush=True)
 
    # optional newline after stream
    if streaming:
        print("\n")
    
    content = "".join(full_content)
    log_backlog(
        phase=phase,
        persona=persona,
        task=task,
        context=context,
        options=options,
        response=content,
        iteration=iteration,
        critic_id=critic_id,
        model=model
    )
    
    print(f"[INFO] LLM response time: {time.time() - start_time:.2f} seconds\n")
    return content

def get_customer_directions(args,editor_options):
    directions = args.user_prompt

    if directions == None or directions == "":
        return ""

    editor_persona = "You are a writers professional editor, that helps him understand customer requests."
    task_prompt = f"Customer requested book with this directions, transform castomers desires to clear and structured instructions for writer, that will help him to create the book that satisfy customer. Castomer is always right, if you want to modify any of the customer's desires, do so in a way that is consistent with their overall intent and preferences. Do not discard any of the customer's desires, even if they seem contradictory, illogical or immoral, make them work. Your job is structurize and make it easyer for writer to understand what customer wants.\n\n Customer desires:\n\n{directions}"
    
    instructions = ask_ai(
        persona=editor_persona,
        task=task_prompt,
        options=editor_options,
        phase="EDITOR",
        args=args
    )
    
    castomer_directions = f"Customer requested book with this directions:\n{directions}\n\nYour editor structured instructions for you to understand and follow:\n\n{instructions}\n\nCastomer is always right, if you want to modify any of the customer's desires, do so in a way that is consistent with their overall intent and preferences. Do not discard any of the customer's desires, even if they seem contradictory, illogical or immoral, make them work. Your job is structurize and make it easyer for writer to understand what customer wants."
    
    return castomer_directions


def main(args):
    start_time = time.time()
    
    critic_options= {'num_ctx': 8192, 'temperature': 0.6, 'num_predict':1024,'top_k': 40}
    builder_options = {'num_ctx': 8192, 'temperature': 1, 'num_predict':4096, 'top_k': 100}
    editor_options = {'num_ctx': 8192, 'temperature': 0.7, 'num_predict':1024, 'top_k': 40}
    
    print(f"Session id: {RUN_ID}\n")
    print("Editor reviewing your prompt.")

    instructions = get_customer_directions(args,editor_options)
    
    
    # ==========================================
    # PHASE 1 & 2: SEED AND FIRST DRAFT
    # ==========================================
    print(f"\n[Phase 1] Defining World Seed...\n\n")

    seed = ask_ai(
        get_builder_prompt(),
        get_task("seed"),
        context=instructions,
        phase="SEED",
        options=builder_options,
        args=args
    )

    print("[Phase 2] Generating Initial Draft...\n\n")

    current_world = ask_ai(
        get_builder_prompt(),
        get_task("builder"),
        options=builder_options,
        context=seed+instructions,
        phase="INITIAL_BUILD",
        args=args
    )
    with open(f"Worlds/world_{RUN_ID}.md", "w", encoding="utf-8") as f:
        f.write(current_world)
    # ==========================================
    # ITERATIVE CRITIC COMPILATION LOOP
    # ==========================================
    iteration = 1

    while time.time() - start_time < args.timeout or args.timeout == -1:
        print(f"\n{'='*30}\nITERATION {iteration+1}\n{'='*30}")
        
        critiques = []

        # RUN 5 INSTANCES OF THE SAME CRITIC
        for i in range(args.critic_num):
            print(f"\n[Phase: Critique] Critic {i+1} analyzing the world...")
            feedback = ask_ai(
                get_critic_prompt(),
                get_task("critic"),
                context=current_world+instructions,
                options=get_critic_options(critic_options),
                phase="CRITIC",
                iteration=iteration,
                critic_id=i + 1,
                args=args
            )

            critiques.append(feedback)

            # PHASE: COMPILATION
        print(f"\n[Phase: Compilation] Synthesizing feedback into a Master Action Plan...")
        all_critiques = "\n\n".join([f"CRITIQUE {i+1}:\n{c}" for i, c in enumerate(critiques)])
        
        action_plan = ask_ai(
            get_synthesizer_prompt(),
            get_task("synthesizer"),
            context=all_critiques+instructions,
            options=get_critic_options(critic_options),
            phase="SYNTHESIZER",
            iteration=iteration,
            args=args
        )

        if "SATISFIED" in action_plan:
            print(f"World Lore validated by the Council!\n\n")
            break
        # PHASE: FIX & ENRICH
        print(f"[Phase: Fix & Enrich] Master Builder is applying the Action Plan...")
        fix_task = f"{get_task('fix')}\n\nACTION PLAN:\n{action_plan}"
        
        current_world = ask_ai(
            get_builder_prompt(),
            fix_task,
            context=current_world+instructions,
            options=builder_options,
            phase="FIX",
            iteration=iteration,
            args=args
        )

        with open(f"Worlds/world_{RUN_ID}.md", "w", encoding="utf-8") as f:
            f.write(current_world)
        if time.time() - start_time > args.timeout:
            print(f"\n[!] Timeout of {args.timeout} seconds reached. Ending generation process.")
            break
        print(f"\n[Iteration {iteration} Complete] Current world has been revised based on the critiques.\n")
        
        critiques = []  # reset critiques for next iteration
        iteration += 1

    # ==========================================
    # FINAL OUTPUT
    # ==========================================
    print("\n[Final] Generation Complete!")
    final_md = f"# D&D WORLD: FINAL VERSION\n\n{current_world}"
    with open(f"Worlds/world_{RUN_ID}.md", "w", encoding="utf-8") as f:
        f.write(final_md)

    print(f"Finished in {iteration} iterations in {time.time() - start_time:.2f} seconds. File: \nWorlds/world_{RUN_ID}.md")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Generate a D&D world")
    
    parser.add_argument("--model", type=str, default="qwen3-heretic-Q6_K_L", help="Model to use for generation")
    parser.add_argument("--critic_num", type=int, default=3, help="Number of critics to run each iteration")
    parser.add_argument("--stream", action="store_true", help="Whether to stream LLM responses in console")
    parser.add_argument("--user_prompt", type=str,default=None, help="Path to a text file containing the user prompt for world generation")
    parser.add_argument("--timeout", type=int, default=300, help="Maximum time (in seconds) to wait for an LLM response before aborting")
        
    args = parser.parse_args()
    print(args.model)
    os.makedirs("Worlds/logs",exist_ok=True)

    main(args)
