import os

with open("master.cs", 'w') as code_file:
    content = ""
    for root, dirs, files in os.walk("./"):
        for file in files:
            if file.endswith(".cs") and not "master" in file:
                print(os.path.join(root, file))
                with open(os.path.join(root, file), 'r') as content_file:
                    content += content_file.read()
                    content += "\n"

    code_file.write(content)