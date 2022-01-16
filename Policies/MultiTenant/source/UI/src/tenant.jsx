/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */
import React, { useState } from "react";

import axios from 'axios';

import { useMsal } from "@azure/msal-react";

import {  ButtonGroup, Button, Table, ToggleButton, ToggleButtonGroup } from "react-bootstrap";

import { b2cPolicies, deployment } from "./authConfig";
import { useEffect } from "react";

export const Tenant = () => {
    const [nowShowing, setState] = useState("claims");    
    const { instance, accounts } = useMsal();
    let options = [["claims","Claims"], ["members", "Members"], ["invitation", "New"], ["myurl", "My url"]].filter(role).map((v) => 
        <Button key={v[0]} onClick={() => setState(v[0])}>{v[1]}</Button>
    );
    function role(option) {
        if(option[0] === "invitation")
            if (accounts[0].idTokenClaims.roles.includes("Tenant.admin")) return true; else return false;
        //if(option[0] === "myurl")
        //    if (accounts[0].idTokenClaims.idp != 'local') return true; else return false;            
        return true;
    }
    return (
        <>
            <div>
                <ButtonGroup className="mb-2">
                    {options}
                </ButtonGroup>                      
            </div>
            {nowShowing === "claims"?
                <IdTokenContent />
            :(nowShowing === "members")?
                <Members instance = {instance} account = {accounts[0]}/>
            :(nowShowing === "invitation")?
                <InviteMember />
            :
                <MyUrl domain_hint={accounts[0].idTokenClaims.idp} login_hint={accounts[0].idTokenClaims.email?? accounts[0].idTokenClaims.signInName} tenant={accounts[0].idTokenClaims.appTenantName}/>
            }            
        </>
    );
}

const IdTokenContent = () => {
    const { accounts } = useMsal();
    const [idTokenClaims, setIdTokenClaims] = useState(accounts[0].idTokenClaims);
    return (
        <>
                <IdTokenClaims idTokenClaims={idTokenClaims} />
        </>
    );
};

const IdTokenClaims = (props) => {  
    return (
        <>
            <h5 className="card-title">Some token claims</h5>
            <Table>
                <thead>
                    <tr key="ix">
                        <th>Name</th>
                        <th>Value</th>
                    </tr>
                </thead>    
                <tbody>
                    <tr>
                        <td><strong>Email/sign in name</strong></td>
                        <td>{props.idTokenClaims.email?? props.idTokenClaims.signInName}</td>
                    </tr> 
                    <tr>
                        <td><strong>Object id</strong></td>
                        <td>{props.idTokenClaims.sub}</td>
                    </tr>   
                    <tr>
                        <td><strong>App tenant name</strong></td>
                        <td>{props.idTokenClaims.appTenantName}</td>
                    </tr>      
                    <tr>
                        <td><strong>App tenant id</strong></td>
                        <td>{props.idTokenClaims.appTenantId}</td>
                    </tr>     
                    <tr>
                        <td><strong>Role(s)</strong></td>
                        <td><ul className="plain-list"><ListRoles roles={props.idTokenClaims.roles} /></ul></td>
                    </tr>                                                                                                          
                </tbody>                                
            </Table>      
            </>  
    );
}

const InviteMember = () => {
    const [email, setEmail] = useState("abc@xyz.com");
    const [invitation, setInvitation] = useState(null);
    const [statusMsg, setStatusMsg] = useState("");
    const { instance, accounts } = useMsal();    
    const [ isAdmin, setIsAdmin ] = useState(false);
    return (
        <div>
            <h5 className="card-title">Invitation</h5>
            <div>
                <div><p><i>Enter email address</i></p>               
                    <div><input type="text" value={email} onChange={(e) => { setEmail(e.target.value); setInvitation(""); setStatusMsg(""); }}/></div>
                </div>                     
                <br />
                <ToggleButton
                    id="isTenantAdmin"
                    type="checkbox"
                    variant="primary"
                    checked={isAdmin}
                    value="0"
                    onChange={(e) => { setIsAdmin(e.currentTarget.checked); setInvitation(""); }} >
                    Is co-admin?
                    </ToggleButton>
                <br />              
                <div><Button onClick={() => 
                    {
                        console.log('starting click' + email);
                        console.log("isAdmin?" + isAdmin);
                        setStatusMsg("generating");
                        //setEmail(email);
                        setInvitation("");
                        let request = { 
                            authority: `https://${deployment.b2cTenantName}.b2clogin.com/${deployment.b2cTenantId}/${accounts[0].idTokenClaims.acr}`,
                            scopes: ["openid", "profile", `https://${deployment.b2cTenantName}.onmicrosoft.com/mtrest/User.Invite`, `https://${deployment.b2cTenantName}.onmicrosoft.com/mtrest/User.ReadAll`],
                            account: accounts[0],
                            extraQueryParameters: { tenant: accounts[0].idTokenClaims.appTenantName }
                        }
                        let callApi = (accessToken) => {
                            axios.post(
                                `${deployment.restUrl}tenant/oauth2/invite`,
                                { inviteEmail: email, additionalClaims: { isAdmin: isAdmin.toString() } },
                                { headers: { 'Authorization': `Bearer ${accessToken}`} }
                              ).then(response => { setInvitation(response.data); console.log("invite received");})
                              .catch(error => console.log(error));                            
                        }
                        instance.acquireTokenSilent(request).then(function(accessTokenResponse) {
                            console.log("Email:"+email);
                            callApi(accessTokenResponse.accessToken);

                        }).catch(function (error) {
                            if (error instanceof InteractionRequiredAuthError) {
                                instance.acquireTokenPopup(request).then(function(accessTokenResponse) {
                                    callApi(acceaccessTokenResponse.accessTokenssToken);
                                }).catch(function(error) {
                                    console.log(error);
                                });
                            }
                            console.log(error);
                        });
                    }}>Invite</Button></div>
                {invitation?
                    <Table bordered="true">
                        <tbody>
                            <p>Copy and send the following link to the invited person.</p>
                            <a href={invitation} target="_blank" rel="noopener">Invitation url</a>
                        </tbody>
                    </Table>
                :statusMsg?
                    <p>Generating invitation link, please wait...</p>                    
                :
                    <p/>
                }       
            </div>
         </div>
    );
};

const Members = (props)  => {
    console.log("Members: " + props);

    const [members, setMembers] = useState(null);
    const { instance, accounts } = useMsal(); 
    const account = accounts[0];   

    const getMembers = (accessToken) => {
        console.log("Starting getMembers");
        axios.get(
            `${deployment.restUrl}tenant/oauth2/members`,
            { headers: { 'Authorization': `Bearer ${accessToken}`} }
        )
        .then(response => { 
            console.log(`${response.data.length} members received`); 
            setMembers(response.data)
            })
        .catch(error => console.log(error));             
    }

    useEffect(() => {
        let request = { 
            authority: `https://${deployment.b2cTenantName}.b2clogin.com/${deployment.b2cTenantId}/${account.idTokenClaims.acr}`,
            scopes: ["openid", "profile", `https://${deployment.b2cTenantName}.onmicrosoft.com/mtrest/User.Invite`, `https://${deployment.b2cTenantName}.onmicrosoft.com/mtrest/User.ReadAll`],
            account: accounts[0],
            extraQueryParameters: { tenant: account.idTokenClaims.appTenantName }
        };
        instance.acquireTokenSilent(request).then(function(accessTokenResponse) {
            getMembers(accessTokenResponse.accessToken);
        }).catch(function (error) {
            if (error instanceof InteractionRequiredAuthError) {
                instance.acquireTokenPopup(request).then(function(accessTokenResponse) {
                    getMembers(accessTokenResponse.accessToken);
                }).catch(function(error) {
                    console.log(error);
                });
            }
            console.log(error);
        });
    },[]) 
    

    return (
        <>
        {members? 
            <div>
                <h5 className="card-title">{`Tenant: ${account.idTokenClaims.appTenantName} has ${members.length} members`}</h5>
                <Table>
                    <thead>
                        <tr key="ix">
                            <th>Email</th>
                            <th>Name</th>
                            <th>Roles</th>
                        </tr>
                    </thead>    
                    <tbody>
                        <ListMembers members={members} />   
                    </tbody>                                
                </Table>
                {members.length < 2?
                <div>
                    <h5>It's lonely here! Please use <strong>New</strong> option above to invite other users</h5>
                    <h5>If this tenant was created with a <i>Work or School</i> account and marked as <i>Allow</i>,</h5>
                    <h5>invitations are not needed for users from the same AAD directory.</h5>
                </div>
                :
                <p/>}
            </div>
        :
            <p>Loading, please wait...</p>
        }
        </>

    )
};

const ListRoles = (props) => {
    return (props.roles.map((r,ix) =>
        <li key={ix}>{r}</li>
    ))
}

const ListMembers = (props) => {
    return (props.members.map((m, ix) =>
        <tr key={ix}>
            <td>{m.email}</td>
            <td>{m.name}</td>
            <td><ul className="plain-list" ><ListRoles roles={m.roles}/></ul></td>
        </tr>
    ))
}

const MyUrl = (props) => {

    console.log(props)
    const lh = props.login_hint;
    const url = 'https://aka.ms/mtb2c?domain_hint=' + props.domain_hint + '&login_hint=' + lh + '&tenant=' + props.tenant;
    return (
        <>
            <h5>Use this url to speed up your sign in next time:</h5>
            <p>{url}</p>
        </>
    )
}

