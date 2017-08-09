## Proposal
Name: NEP-5<br/>
Title:Token Standard<br/>
Author: luodanwg <luodan.wg@gmail.com>, tanyuan <tanyuan666@gmail.com><br/>
Status: Draft<br/>
Created: 9 August 2017<br/>
Resolution: <a href="https://github.com/neo-project/proposals/pull/1"> Proposals </a><br/>
Template:  <a href="https://github.com/tanZiWen/neo/wiki/NEO_NEP_5">NEO_NEP_5</a><br/>
<h2> Abstract </h2>
<p>The following describes standard functions a token contract can implement.</p>
<h2> Specification </h2>
Token<br/>
Contracts that work with tokens
<h2> Methods </h2>
<p>NOTE: The contract developers must implement all function if they want to work with the specified tokens. </p>
<p><strong> totalSupply </strong></p>
<pre><code>private static BigInteger totalSupply()</pre></code>
<p>Get the total token supply</p>

<p><strong> name </strong></p>
<pre><code>private static string name()</pre></code>
<p>Get the name of token</p>

<p><strong> symbol </strong></p>
<pre><code>private static string symbol()</pre></code>
<p>Get the symbol of token, symbol used to represent a unit of token</p>

<p><strong> decimals </strong></p>
<pre><code> private static BigInteger decimals()</pre></code>
<p>Get decimals of token</p>

<p><strong> balanceOf </strong></p>
<pre><code> private static BigInteger balanceOf(object[] args) </pre></code>
<p>Get the account balance of another account with address which is first element of args and type is <strong>byte[]</strong></p>

<p><strong> transfer </strong></p>
<pre><code> private static bool transfer(object[] args) </pre></code>
<p>function that is always called when someone wants to transfer tokens. The first element is sender address and type is <strong>byte[]</strong>, the second element is receiver address and type is <strong>byte[]</strong>, the third element is the number of token and type is <strong> BigInteger </strong>. </p>

<h2> Event </h2>
<p><strong> Transfer </strong></p>
<pre><code> private static void Transfer(object[] args)</pre></code>
<p>Triggered when tokens are transferred.</p>