<%@ Page Title="Demo" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AspNetWebFormsRestConsumer._Default" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
    </asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <br />
   <h1>Welcome to Rest Using Asp.Net</h1>
    <h4>This is not meant to be a complex sample. It uses basic controls and uploads to show how you can integrate with DCTM Rest Services<br />
        at even the most basic level. There are toolkits that make working with Rest services very easy and you can build more complex UIs<br />
        with them.
    <p>
        <a runat="server" href="~">Home - Is this page</a> <br />
        <a runat="server" href="~/DQL">DQL - Is a page where you can enter a simple qualifer and get query results back.</a> <br />
        <a runat="server" href="~/Create">Create - This page will allow you to create an object and file it to a specific path or use a D2 Profile to automate its creation.</a> <br />
    <br />
        </p>
</asp:Content>
