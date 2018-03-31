# TrustPilot anagram
Anagram challenge from trustpilot. :)

http://followthewhiterabbit.trustpilot.com/cs/step3.html


Notes
--
The challenge was really interesting and funny :D
<br>I choose to use hashsets just to remember how to work with it. And still there are many ways to improve the code or to do it differently as You would say :)

(Using i5-7600k)
It takes 1,2 sec to find Easy and Medium phrases(three words anagram).
It takes ~4min to find Hard phrase (four words anagram).

Algorithm (This algorithm works only when anagram contains 3 or 4 words) 
--

1. Filter wordlist.txt to only contain the words that we want.
ex. so that there are no different letters between anagram and wordlist

2. Find all possible combinations of 3 words that contains exact number of letters count as it is in anagram.

3. For all the possible combinations we should get all possible permutations of it.

4. All of these possible permutations should be hashed with MD5 and check if it's matching the main anagram hash.

5. Repeat from 2# with change that combination should contain 4 words (for "Hard" phrase).
