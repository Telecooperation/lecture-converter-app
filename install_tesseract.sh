cd x64

sudo apt install libleptonica-dev
ln -s /usr/lib/x86_64-linux-gnu/liblept.so.5 liblept.so.5
ln -s /usr/lib/x86_64-linux-gnu/liblept.so.5 libleptonica-1.78.0.so

# Installed the repository from https://notesalexp.org/
sudo apt install libtesseract-dev
ln -s /usr/lib/x86_64-linux-gnu/libtesseract.so.4.0.1 libtesseract41.so