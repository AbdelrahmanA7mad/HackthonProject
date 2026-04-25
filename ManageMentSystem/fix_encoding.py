import sys
import codecs

filepath = r"c:\Users\Abdo\Desktop\qota\SaaS-System\ManageMentSystem\Controllers\StoreAccountController.cs"

with open(filepath, 'r', encoding='utf-8') as f:
    lines = f.readlines()

fixed_lines = []
for line in lines:
    try:
        # Check if the line has non-ASCII characters
        if any(ord(c) > 127 for c in line):
            # Attempt to decode as if it was windows-1252 text misinterpreted as utf-8
            # Only do this if we find the specific mojibake signature (e.g., 'Ø')
            if 'Ø' in line or 'Ù' in line:
                fixed_line = line.encode('windows-1252').decode('utf-8')
                fixed_lines.append(fixed_line)
                continue
    except Exception as e:
        pass
    fixed_lines.append(line)

with open(filepath, 'w', encoding='utf-8-sig') as f:
    f.writelines(fixed_lines)

print("Encoding fixed successfully!")
