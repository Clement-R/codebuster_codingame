import os
import pyperclip

if __name__ == "__main__":
    try:
        os.chdir("E:\_Dev\AI_codebuster")
    except Exception:
        print("No file")

    try:
        os.chdir("C:\AI\codebuster_codingame")
    except Exception:
        print("No file")

    with open("master.cs", 'w') as code_file:
        content = """
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

"""
        for root, dirs, files in os.walk("./"):
            for file in files:
                if file.endswith(".cs") and not "master" in file and not "Temporary" in file:
                    print(os.path.join(root, file))
                    with open(os.path.join(root, file), 'r') as content_file:
                        copied_content = [line for line in content_file.readlines() if "using" not in line]
                        content += "".join(copied_content).replace("ï»¿", "")
                        content += "\n"
                        print(content)
        code_file.write(content)
        pyperclip.copy(content)

