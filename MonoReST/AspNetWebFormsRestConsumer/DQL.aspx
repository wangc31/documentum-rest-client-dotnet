<%@ Page Title="DQL" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DQL.aspx.cs" Inherits="AspNetWebFormsRestConsumer.DQL" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>Qualifier Query<br />
            Provide only the type and criteria (Eg. dm_document where folder(&#39;/Temp&#39;)<br />
        </h1>
    </hgroup>

    <article>
        <p id="Query">        
           DQL Qualifier:<br />
        <asp:TextBox ID="txtQuery" runat="server" Width="1390px">dm_document where folder(&#39;/Temp&#39;)</asp:TextBox>

            <br />

            <asp:Button ID="btnExecuteQuery" runat="server" Text="Execute Query" Height="32px" OnClick="btnExecuteQuery_Click" Width="294px" />

        </p>
        <p>        
        <asp:GridView ID="gridView" runat="server" EmptyDataText="No Results"/>

        </p>

    </article>
    </asp:Content>