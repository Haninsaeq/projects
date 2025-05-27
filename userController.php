<?php

namespace App\Http\Controllers;
use Illuminate\Support\Facades\Session;
use Illuminate\Http\Request;
use App\Models\User;
use Illuminate\Support\Facades\Hash;
use App\Models\Service;
use App\Models\Image;
use Illuminate\Support\Facades\Mail;
use App\Mail\NewUserRegistered;
use App\Mail\ForgotPasswordMail;
use App\Models\Guest;

class userController extends Controller
{
    public function index()
    {
        $services = Service::all(); // Fetch services;
        $images = Image::all();
        return view('home', ['service' => $services,
        'images' => $images]); 
    }

    public function signup()
    {
        return view('signup');
    }

    public function login()
    {
        return view('login');
    }

   
     
    public function fn(Request $request)
    {
        // Validate the incoming request data
        $request->validate([
            'email' => 'required|email',
            'password' => 'required',
        ]);

        // Retrieve the email and password from the request
        $name = $request->input('name');
        $email = $request->input('email');
        $password = $request->input('password');

        // Check if the user exists
        $user = User::where('brideemail', $email)->first();
        if ($user) { // Check if the user exists
            if ($user->email_verified_at === null) {
                // The user is not verified
                dd('You are not verified');   
            } else {
                if ($user && Hash::check($password, $user->password)) {
                    Session::put('emailbride',$email);
                    Session::put('namebride',$name);
                    $card = Guest::where('email', $email)->first();
                    if($card){
                        Session::put('cardtoken',$card->bridetoken);
                    }
                    return redirect('/');
        
                }
                
            }
        } else {
            // Handle the case where the user does not exist
            dd('No user found with that email.');
        }
        // Verify password
    
        

        return response()->json(['message' => 'Invalid credentials'], 401);
    }


    public function store(Request $request)
    {    
        request()->validate([
         'bridename'=>['required','max:10'],
         'groomname'=>['required','max:20'],
         
          'password'=>['required','max:20']
        ]);

       if( request('password') === request('passwordconfirm'))
       {
        $user=User::create([
            'bridename'=> request('bridename'),
            'groomname'=> request('groomname'),
            'groomemail'=> request('groomemail'),
            'brideemail'=> request('brideemail'),
            'brideage'=> request('brideage'),
            'groomage'=> request('groomage'),
            'password'=> Hash::make($request->password),
            'passwordconfirm'=> request('passwordconfirm'),
        ]);
        }else {
            return redirect()->back()->with('faild', 'password and password confirmation not match');
        }

       
        Session::put('namebride',request('bridename'));
        Session::put('brideemail',request('brideemail'));
        // $user->sendEmailVerificationNotification();
        Mail::to('haninalsaeq@gmail.com')->send(new NewUserRegistered($user));
        return response()->json(['message' => 'User created successfully. Please check your email for verification.']);

    }


    public function ver()
    {
        return view('ver');
    }

    public function verpost(Request $request)

    { 
       // Retrieve the user based on the brideemail from the session
        $user = User::where('brideemail', Session::get('brideemail'))->first(); // Use first() to get the user instance

        if ($user && request('tok') === $user->remember_token) { // Check if user exists and compare tokens
            // Update the user's email_verified_at field
            $user->update(['email_verified_at' => now()]); // Use now() to set the current timestamp
            $services = Service::all(); // Fetch services;
            return view('home', ['service' => $services]); 
        }else{
            dd('your token is wrong');
        }

    }


    public function logout(Request $request)
    {
        Session::flush(); // Clear all session data
        return redirect('/'); // Redirect to the home page or any other page
    }

    public function search()
    {
        $search_text = $_GET['query'];
        $services= Service::where('name','LIKE','%'.$search_text.'%')->orWhere('actor','LIKE','%'.$search_text.'%')->get();
        return view('home',['service'=>$services]);
    }

    public function forgetpassword()
    {
        return view('passwords.forgetpassword');
    }

    public function forgetpasswordsend(Request $request)
    {
        $user = User::where('brideemail', request('email'))->first();
        Session::put('email',request('email'));
        if ($user) {
            Mail::to('haninalsaeq@gmail.com')->send(new ForgotPasswordMail($user));
            return back()->with('status', 'Password sent to your email!');

        }else {
            dd('you are not register before ');
        }

        return back()->withErrors(['email' => 'No user found with that email.']);
    }
    
    public function newpassword()
    {
        return view('passwords.newpassword');
    }

    public function newpasswordpost(Request $request)

    {
        $user = User::where('brideemail', Session::get('email'))->first();
        if(request('password') === request('passwordconf')){
        $user->update(['password' => Hash::make($request->password)]);
        Session::flush();
        return view('login');
        }else {
            dd('password and password confirmation not matching ');
        }
    }

    public function userprofile()
    {    
        $image= Image::all();
        return view('userprofile',['images'=>$image]);
    }

}
